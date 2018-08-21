﻿using System.IO;
using Kanban.Desktop.LocalBase.ViewModels;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace Kanban.Desktop.LocalBase.Views
{
    public partial class BoardView : IView
    {
        public BoardView(BoardViewModel localBoardViewModel)
        {
            InitializeComponent();
            ViewModel = localBoardViewModel;
            DataContext = ViewModel;
        }

        public IViewModel ViewModel { get; set; }

        public void Configure(UiShowOptions options)
        {
            ViewModel.FullTitle = options.Title;
            ViewModel.Title = Path.GetFileName(options.Title);
        }
    }
}
