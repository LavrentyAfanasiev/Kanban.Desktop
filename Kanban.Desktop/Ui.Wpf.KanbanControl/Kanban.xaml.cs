﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ui.Wpf.KanbanControl.Behaviours;
using Ui.Wpf.KanbanControl.Dimensions;
using Ui.Wpf.KanbanControl.Elements;
using Ui.Wpf.KanbanControl.Expressions;

namespace Ui.Wpf.KanbanControl
{
    /// <summary>
    /// codebehind for Kanban.xaml
    /// </summary>
    public partial class Kanban : IKanbanBoard
    {
        public Kanban()
        {
            showKanbanStrategy = new DefaultShowKanbanStrategey(this);

            InitializeComponent();
        }

        private void AddActionsToShow(KanbanChangeObjectType changeObjectType)
        {
            // ugly destory and build
            // TODO beautiful transition
            // TODO store changesets and animate last little part of it with some low frequency

            if (HorizontalDimension == null
                || VerticalDimension == null
                || CardItems == null)
                return;

            // TODO create only of type changed
            propertyAccessors = new PropertyAccessorsExpressionCreator(CardItems);
            BuildAutoCategories(CardItems, HorizontalDimension, VerticalDimension, propertyAccessors);

            if (HorizontalDimension.Categories == null
                || HorizontalDimension.Categories.Count == 0
                || VerticalDimension.Categories == null
                || VerticalDimension.Categories.Count == 0)
                return;


            BuildCards();
            BuildCells();
            BuildHeaders();            

            showKanbanStrategy.AddActionsToShow(
                changeObjectType, 
                propertyAccessors);
        }

        private void BuildAutoCategories(
            IEnumerable cardItems, 
            IDimension horizontalDimension, 
            IDimension verticalDimension, 
            PropertyAccessorsExpressionCreator propertyAccessors)
        {
            if (horizontalDimension is IDynamicDimension)
            {
                horizontalDimension.Categories = GetDimensionCategories(cardItems, horizontalDimension, propertyAccessors);
            }

            if (verticalDimension is IDynamicDimension)
            {
                verticalDimension.Categories = GetDimensionCategories(cardItems, verticalDimension, propertyAccessors);
            }
        }

        private static IList<IDimensionCategory> GetDimensionCategories(
            IEnumerable cardItems, 
            IDimension dimension, 
            PropertyAccessorsExpressionCreator propertyAccessors)
        {
            var getElementCategory = propertyAccessors.TakeGetterForProperty(dimension.FieldName);

            if (getElementCategory != null)
            {
                var categories = cardItems.Cast<object>()
                    .Select(i => getElementCategory(i))
                    .Where(i => i != null)
                    .OrderBy(i => i)
                    .Distinct()
                    .Select(i => (IDimensionCategory)new TagsDimensionCategory(i.ToString(), i))
                    .ToList();

                return categories;
            }

            return null;
        }

        private void BuildCells()
        {
            cells = new Cell[HorizontalDimension.Categories.Count, VerticalDimension.Categories.Count];
            for (int i = 0; i < HorizontalDimension.Categories.Count; i++)
            {
                for (int j = 0; j < VerticalDimension.Categories.Count; j++)
                {
                    cells[i, j] = new Cell(new CellView());
                }
            }
        }

        private void SetCardItems(object oldValue, object newValue)
        {
            var oldNotifyableCollection = (INotifyCollectionChanged)oldValue;
            if (oldNotifyableCollection != null)
                oldNotifyableCollection.CollectionChanged -= OnItemsChanged;

            var newNotifyableCollection = (INotifyCollectionChanged)newValue;
            if (newNotifyableCollection != null)
                newNotifyableCollection.CollectionChanged += OnItemsChanged;

            AddActionsToShow(KanbanChangeObjectType.CardItems);
        }

        private void OnItemsChanged(
            object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            AddActionsToShow(KanbanChangeObjectType.CardItems);
        }

        private void BuildCards()
        {
            cards.Clear();
            foreach (var item in CardItems)
            {
                var card = new Card(item);
                card.View.ContentTemplate = CardItemTemplate;

                cards.Add(card);
            }
        }

        private void BuildHeaders()
        {
            horizontalHeaders.Clear();
            for (int i = 0; i < HorizontalDimension.Categories.Count; i++)
            {
                var head = new HeaderView();
                head.ContentTemplate = HorizontalHeaderTemplate;
                head.Content = HorizontalDimension.Categories[i];
                horizontalHeaders.Add(new Header(head));
            }

            verticalHeaders.Clear();
            for (int j = 0; j < VerticalDimension.Categories.Count; j++)
            {
                var head = new HeaderView();
                head.ContentTemplate = VerticalHeaderTemplate;
                head.Content = VerticalDimension.Categories[j];
                verticalHeaders.Add(new Header(head));
            }
        }

        #region [ IKanbanBoard ]

        private readonly List<Header> verticalHeaders = new List<Header>();
        List<Header> IKanbanBoard.VerticalHeaders => verticalHeaders;

        private readonly List<Header> horizontalHeaders = new List<Header>();
        List<Header> IKanbanBoard.HorizontalHeaders => horizontalHeaders;

        private readonly List<Card> cards = new List<Card>();
        List<Card> IKanbanBoard.Cards => cards;

        private Cell[,] cells;
        Cell[,] IKanbanBoard.Cells => cells;

        Grid IKanbanBoard.KanbanGrid => Grid;

        #endregion

        #region [ properties ]

        private static readonly DefaultTemplates defaultTemplates = new DefaultTemplates();

        //TODO dependency properties

        private IShowKanbanStrategy showKanbanStrategy;
        private PropertyAccessorsExpressionCreator propertyAccessors;

        public IShowKanbanStrategy ShowKanbanStrategy
        {
            get => showKanbanStrategy;
            set
            {
                showKanbanStrategy = value;
                AddActionsToShow(KanbanChangeObjectType.ShowStrategy);
            }
        }

        #endregion

        #region [ dependency properties ]

        public IEnumerable CardItems
        {
            get => (IEnumerable)GetValue(CardItemsProperty);
            set => SetValue(CardItemsProperty, value);
        }

        public static readonly DependencyProperty CardItemsProperty =
            DependencyProperty.Register("CardItems", 
                typeof(IEnumerable), 
                typeof(Kanban), 
                new PropertyMetadata(
                    new ObservableCollection<object>(),
                    OnCardItemsChanged));

        private static void OnCardItemsChanged(
            DependencyObject obj, 
            DependencyPropertyChangedEventArgs e)
        {
            var control = (Kanban)obj;
            control.SetCardItems(e.OldValue, e.NewValue);
        }

        public IDimension VerticalDimension
        {
            get => (IDimension)GetValue(VerticalDimensionProperty);
            set => SetValue(VerticalDimensionProperty, value);
        }

        public static readonly DependencyProperty VerticalDimensionProperty =
            DependencyProperty.Register("VerticalDimension", 
                typeof(IDimension), 
                typeof(Kanban), 
                new PropertyMetadata(
                    null,
                    OnVerticalDimensionChanged));

        private static void OnVerticalDimensionChanged(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (Kanban)obj;
            control.AddActionsToShow(KanbanChangeObjectType.VerticalCategories);
        }

        public IDimension HorizontalDimension
        {
            get => (IDimension)GetValue(HorizontalDimensionProperty);
            set => SetValue(HorizontalDimensionProperty, value);
        }

        public static readonly DependencyProperty HorizontalDimensionProperty =
            DependencyProperty.Register("HorizontalDimension",
                typeof(IDimension),
                typeof(Kanban),
                new PropertyMetadata(
                    null,
                    HorizontalDimensionChanged));

        private static void HorizontalDimensionChanged(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (Kanban)obj;
            control.AddActionsToShow(KanbanChangeObjectType.HorizontalCategories);
        }

        public double SpliterWidth
        {
            get => (double)GetValue(SpliterWidthProperty);
            set => SetValue(SpliterWidthProperty, value);
        }

        public static readonly DependencyProperty SpliterWidthProperty =
            DependencyProperty.Register("SpliterWidth", typeof(double), typeof(Kanban), new PropertyMetadata(3d));

        public Brush SpliterBackground
        {
            get => (Brush)GetValue(SpliterBackgroundProperty);
            set => SetValue(SpliterBackgroundProperty, value);
        }

        public static readonly DependencyProperty SpliterBackgroundProperty =
            DependencyProperty.Register("SpliterBackground", typeof(Brush), typeof(Kanban), new PropertyMetadata(Brushes.WhiteSmoke));


        public DataTemplate CardItemTemplate
        {
            get => (DataTemplate)GetValue(CardItemTemplateProperty);
            set => SetValue(CardItemTemplateProperty, value);
        }

        public static readonly DependencyProperty CardItemTemplateProperty =
            DependencyProperty.Register("CardItemTemplate", 
                typeof(DataTemplate), 
                typeof(Kanban), 
                new PropertyMetadata());

        
        public DataTemplate CellTemplate
        {
            get => (DataTemplate)GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        public static readonly DependencyProperty CellTemplateProperty =
            DependencyProperty.Register("CellTemplate", 
                typeof(DataTemplate), 
                typeof(Kanban), 
                new PropertyMetadata());
        
        public DataTemplate HorizontalHeaderTemplate
        {
            get => (DataTemplate)GetValue(HorizontalHeaderTemplateProperty);
            set => SetValue(HorizontalHeaderTemplateProperty , value);
        }

        public static readonly DependencyProperty HorizontalHeaderTemplateProperty =
            DependencyProperty.Register("HorizontalHeaderTemplate", 
                typeof(DataTemplate), 
                typeof(Kanban), 
                new PropertyMetadata(
                    defaultTemplates["DefaultHorizontalHeaderTemplate"]));
        
        public DataTemplate VerticalHeaderTemplate
        {
            get => (DataTemplate)GetValue(VerticalHeaderTemplateProperty);
            set => SetValue(VerticalHeaderTemplateProperty , value);
        }

        public static readonly DependencyProperty VerticalHeaderTemplateProperty =
            DependencyProperty.Register("VerticalHeaderTemplate", 
                typeof(DataTemplate), 
                typeof(Kanban), 
                new PropertyMetadata(
                    defaultTemplates["DefaulVerticalHeaderTemplate"]));
        
        #endregion
    }
}
