﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities.Common.LocalBase;
using Data.Sources.LocalStorage.Sqlite;
using Kanban.Desktop.LocalBase.Views;
//using Kanban.Desktop.LocalBase.ViewModels;
using Ui.Wpf.Common;
using Ui.Wpf.KanbanControl.Dimensions;
using Ui.Wpf.KanbanControl.Dimensions.Generic;
using Ui.Wpf.KanbanControl.Elements.CardElement;

namespace Kanban.Desktop.LocalBase.Models
{
    public class BoardModel
    {
        private readonly SqliteLocalRepository repository;
        private readonly IShell shell;

        private List<RowInfo> rows = new List<RowInfo>();
        private List<ColumnInfo> columns = new List<ColumnInfo>();

        public BoardModel(SqliteLocalRepository repository, IShell shell)
        {
            this.repository = repository;
            this.shell      = shell;
        }

        #region GettingInfo

        public async Task<IDimension> GetColumnHeadersAsync()
        {
            columns.Clear();
            columns = await repository.GetColumnsAsync();

            var columnHeaders = columns.Select(c => c.Name).ToArray();
            return new TagDimension<string, LocalIssue>(
                tags: columnHeaders,
                getItemTags: i => new[] {i.Column.Name},
                categories: columnHeaders
                    .Select(c => new TagsDimensionCategory<string>(c, c))
                    .Select(tdc => (IDimensionCategory) tdc)
                    .ToArray());
        }

        public async Task<IDimension> GetRowHeadersAsync()
        {
            rows.Clear();
            rows = await repository.GetRowsAsync();

            var rowHeaders = rows.Select(r => r.Name).ToArray();
            return new TagDimension<string, LocalIssue>(
                tags: rowHeaders,
                getItemTags: i => new[] {i.Row.Name},
                categories: rowHeaders
                    .Select(r => new TagsDimensionCategory<string>(r, r))
                    .Select(tdc => (IDimensionCategory) tdc)
                    .ToArray()
            );
        }

        public async Task<IEnumerable<LocalIssue>> GetIssuesAsync()
        {
            return await repository.GetIssuesAsync(new NameValueCollection());
        }

        public CardContent GetCardContent()
        {
            return new CardContent(new ICardContentItem[]
            {
                new CardContentItem("Head"),
                new CardContentItem("Body"),
            });
        }

        public RowInfo GetSelectedRow(string rowName)
        {
            return rows.FirstOrDefault(r => r.Name == rowName);
        }

        public ColumnInfo GetSelectedColumn(string colName)
        {
            return columns.FirstOrDefault(c => c.Name == colName);
        }

        #endregion

        #region DeletingInfo

        public async Task DeleteIssueAsync(int issueId)
        {
            await repository.DeleteIssueAsync(issueId);
        }

        public async Task DeleteRowAsync(int rowId)
        {
            await repository.DeleteRowAsync(rowId);
        }

        public async Task DeleteColumnAsync(int columnId)
        {
            await repository.DeleteColumnAsync(columnId);
        }

        #endregion

        public void ShowIssueView(LocalIssue issue)
        {
            shell.ShowView<IssueView>(
                viewRequest: new IssueViewRequest() { IssueId = issue.Id });
        }

        public async Task CreateOrUpdateColumn(ColumnInfo column)
        {
           await repository.CreateOrUpdateColumnAsync(column);
        }

        public async Task CreateOrUpdateRow(RowInfo row)
        {
            await repository.CreateOrUpdateRowAsync(row);
        }
    }
}
