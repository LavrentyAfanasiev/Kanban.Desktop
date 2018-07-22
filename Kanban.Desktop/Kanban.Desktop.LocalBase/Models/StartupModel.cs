﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Data.Sources.LocalStorage.Sqlite;
using Kanban.Desktop.LocalBase.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace Kanban.Desktop.LocalBase.Models
{
    public class StartupModel
    {
        private readonly IShell shell;
        private IDataBaseSettings dataBaseSettings;

        public StartupModel(IShell shell)
        {
            this.shell = shell;
            dataBaseSettings = shell.Container.Resolve<IDataBaseSettings>();
        }

        public string CreateDatabase()
        {
            var saveDialog = new SaveFileDialog()
            {
                Filter = "SQLite DataBase | *.db",
                Title = "Создание базы"
            };
            saveDialog.ShowDialog();

            if (!string.IsNullOrEmpty(saveDialog.FileName))
            {
                var name = saveDialog.FileName;
                ShiftOrCreateBaseList(name);
                return name;
            }

            return null;
        }

        public string OpenDatabase()
        {
            var openDialog = new OpenFileDialog()
            {
                Filter = "SQLite DataBase | *.db",
                Title = "Открытие базы"
            };
            openDialog.ShowDialog();

            if (!string.IsNullOrEmpty(openDialog.FileName))
            {
                var name = openDialog.FileName;
                ShiftOrCreateBaseList(name);
                return name;
            }

            return null;
        }

        public List<string> GetBaseList()
        {
            var path = Directory.GetCurrentDirectory() + "\\BaseList.txt";

            if (File.Exists(path))
                return File.ReadAllLines(path).ToList();

            File.Create(path).Close();
            return new List<string>();
        }

        public bool CheckDataBaseExists(string basePath)
        {
            var exists = File.Exists(basePath);
            if (!exists)
            {
                var path = Directory.GetCurrentDirectory() + "\\BaseList.txt";
                var baseList = File.ReadAllLines(path).ToList();
                baseList.Remove(basePath);
                File.WriteAllLines(path, baseList);
            }

            return exists;
        }

        public void ShowSelectedBaseTab(string path)
        {
            dataBaseSettings.BasePath = path;

            shell.ShowView<BoardView>(options: new UiShowOptions
            {
                Title = "Работа с базой " + path
                            .Substring(path.LastIndexOf('\\') + 1)
                            .TrimEnd(".db".ToCharArray())
            });
        } //TODO: add re-login within one session (-singletone,+some auto-taking basename when resolving)

        public void ShiftOrCreateBaseList(string newBasePath)
        {
            if (string.IsNullOrEmpty(newBasePath)) return;

            var path = Directory.GetCurrentDirectory() + "\\BaseList.txt";

            var baseList = File.ReadAllLines(path).ToList();

            if (baseList.Contains(newBasePath))
            {
                baseList.RemoveAll(existingPath => existingPath == newBasePath);
                baseList.Add(newBasePath);
                File.WriteAllLines(path, baseList);
                return;
            }

            if (baseList.Count < 3)
            {
                baseList.Add(newBasePath);
                File.WriteAllLines(path, baseList);
            }
            else
            {
                baseList[0] = baseList[1];
                baseList[1] = baseList[2];
                baseList[2] = newBasePath;
                File.WriteAllLines(path, baseList);
            }
        }
    }
}
