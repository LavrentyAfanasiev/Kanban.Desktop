﻿using Data.Entities.Common.Redmine;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Ui.Wpf.KanbanControl.Dimensions;

namespace Kanban.Desktop.KanbanBoard
{
    public interface IKanbanBoardViewModel
    {
        ObservableCollection<Issue> Issues { get; }

        TagDimension<string, Issue> VerticalDimension { get; }

        TagDimension<string, Issue> HorizontalDimension { get; }
    }

    public class KanbanBoardViewModel : ReactiveObject, IKanbanBoardViewModel
    {
        public KanbanBoardViewModel(IKanbanConfigurationRepository kanbanRepository)
        {
            var data = kanbanRepository.GetKanbanData();

            VerticalDimension = data.VerticalDimension;
            HorizontalDimension = data.HorizontalDimension;
            Issues = new ObservableCollection<Issue>(data.Issues);
        }

        [Reactive] public ObservableCollection<Issue> Issues { get; private set; }

        [Reactive] public TagDimension<string, Issue> VerticalDimension { get; private set; }

        [Reactive] public TagDimension<string, Issue> HorizontalDimension { get; private set; }
    }
}