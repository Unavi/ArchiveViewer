using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using ArchiveViewer.Models;
using Caliburn.Micro;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using Newtonsoft.Json;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Screen = Caliburn.Micro.Screen;

namespace ArchiveViewer.UserControls.Home
{
    public class HomeViewModel : Screen
    {
        private readonly ArchiveParser _archiveParser;
        private readonly CsvImportExport _csvImportExport;
        private BindableCollection<ArchiveItem> _archiveItems;

        private CollectionViewSource _archiveItemsFiltered;

        private string _filter;
        private BindableCollection<string> _languages;
        private string _projectPath = "projects.json";

        private BindableCollection<Project> _projects;

        private int _selectedArchiveItemCount;

        private BindableCollection<ArchiveItem> _selectedArchiveItems;

        private string _selectedLanguage;
        private Project _selectedProject;

        public HomeViewModel()
        {
            Projects = new BindableCollection<Project>();
            Languages = new BindableCollection<string>();
            ArchiveItems = new BindableCollection<ArchiveItem>();
            SelectedArchiveItems = new BindableCollection<ArchiveItem>();

            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ArchiveViewer\\";
            _projectPath = folderPath + _projectPath;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(_projectPath))
            {
                string projects = string.Join("", File.ReadAllLines(_projectPath));
                Projects.AddRange(JsonConvert.DeserializeObject<Project[]>(projects));
            }

            _archiveParser = new ArchiveParser();
            _csvImportExport = new CsvImportExport();

            _archiveItemsFiltered = new CollectionViewSource
            {
                Source = ArchiveItems
            };
            _archiveItemsFiltered.Filter += ArchiveItemsFilteredOnFilter;
        }

        public BindableCollection<ArchiveItem> SelectedArchiveItems
        {
            get => _selectedArchiveItems;
            set
            {
                if (_selectedArchiveItems == value) return;
                _selectedArchiveItems = value;
                NotifyOfPropertyChange();
            }
        }

        public string Filter
        {
            get => _filter;
            set
            {
                if (_filter == value) return;
                _filter = value;
                NotifyOfPropertyChange();
                ArchiveItemsFiltered.View.Refresh();
            }
        }

        public CollectionViewSource ArchiveItemsFiltered
        {
            get => _archiveItemsFiltered;
            set
            {
                if (_archiveItemsFiltered == value) return;
                _archiveItemsFiltered = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                if (_projects == value) return;
                _projects = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<string> Languages
        {
            get => _languages;
            set
            {
                if (_languages == value) return;
                _languages = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<ArchiveItem> ArchiveItems
        {
            get => _archiveItems;
            set
            {
                if (_archiveItems == value) return;
                _archiveItems = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage == value) return;
                _selectedLanguage = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => CanSaveChanges);
                ArchiveItems.Clear();

                if (SelectedLanguage != null)
                {
                    try
                    {
                        ArchiveItems.AddRange(_archiveParser.ParseArchive(SelectedProject, SelectedLanguage));
                    }
                    catch (InvalidArchiveFolderException)
                    {
                        Languages.Clear();
                        SelectedProject = null;
                    }
                }
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject == value) return;
                _selectedProject = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => CanSaveSelectedProject);
                NotifyOfPropertyChange(() => CanExportCsv);
                NotifyOfPropertyChange(() => CanImportCsv);
            }
        }

        public bool CanSaveChanges => SelectedProject != null && !string.IsNullOrEmpty(SelectedLanguage);

        public bool CanSaveSelectedProject => SelectedProject != null && SelectedProject.Name != "";

        public bool CanImportCsv => SelectedProject != null;
        public bool CanExportCsv => SelectedProject != null;

        public int SelectedArchiveItemCount
        {
            get => _selectedArchiveItemCount;
            set
            {
                if (_selectedArchiveItemCount == value) return;
                _selectedArchiveItemCount = value;
                NotifyOfPropertyChange();
            }
        }

        public void SelectedArchiveItemsChanged()
        {
            SelectedArchiveItemCount = SelectedArchiveItems.Count;
        }

        public void AutoTranslateSelected()
        {
            string token = File.ReadAllText("ArchiveViewer.json");
            GoogleCredential googleCredential = GoogleCredential.FromJson(token);
            TranslationClient translationClient = TranslationClient.Create(googleCredential);

            string[] toTranslate = SelectedArchiveItems.Select(d => d.Native).ToArray();

            if (toTranslate.Length == 0)
            {
                MessageBox.Show("Nothing selected", "Error", MessageBoxButtons.OK);
                return;
            }

            if (toTranslate.Length > 125)
            {
                MessageBox.Show("Can't translate more than 125 entries with one call.", "Error", MessageBoxButtons.OK);
                return;
            }

            try
            {
                IList<TranslationResult> results = translationClient.TranslateText(
                    toTranslate,
                    SelectedLanguage);
                for (var i = 0; i < results.Count; i++)
                {
                    TranslationResult result = results[i];
                    SelectedArchiveItems[i].Translated = result.TranslatedText;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }

        private void ArchiveItemsFilteredOnFilter(object sender, FilterEventArgs filterEventArgs)
        {
            filterEventArgs.Accepted = false;
            if (string.IsNullOrEmpty(Filter))
            {
                filterEventArgs.Accepted = true;
                return;
            }

            if (filterEventArgs.Item is ArchiveItem archiveItem)
            {
                if (archiveItem.Namespace.Contains(Filter))
                {
                    filterEventArgs.Accepted = true;
                    return;
                }

                if (archiveItem.Key.Contains(Filter))
                {
                    filterEventArgs.Accepted = true;
                    return;
                }

                if (archiveItem.Native.Contains(Filter))
                {
                    filterEventArgs.Accepted = true;
                    return;
                }

                if (archiveItem.Translated.Contains(Filter))
                {
                    filterEventArgs.Accepted = true;
                }
            }
        }

        public void SaveChanges()
        {
            _archiveParser.SaveArchive(SelectedProject, SelectedLanguage, ArchiveItems);
        }

        public void SavePathToClipBoard()
        {
            if (SelectedProject != null)
            {
                Clipboard.SetText(SelectedProject.Path);
            }
        }

        public void SelectFolder()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Manifest (.manifest)|*.manifest";

            bool? result = dlg.ShowDialog();
            if (result == false)
            {
                return;
            }

            SelectedProject =
                new Project("DefaultName", dlg.FileName.Replace("\\" + Path.GetFileName(dlg.FileName), ""));
            LoadProject();
        }

        public void SaveSelectedProject()
        {
            var noProjectFound = Projects.FirstOrDefault(d => d.Path == SelectedProject.Path) == null;
            if (noProjectFound)
            {
                Projects.Add(SelectedProject);
            }

            SaveProjects();
        }

        public void DeleteProject(string projectName, string projectPath)
        {
            var project = Projects.First(d => d.Name == projectName && d.Path == projectPath);
            Projects.Remove(project);
            SaveProjects();
        }

        public void SelectProject(string projectName, string projectPath)
        {
            var project = Projects.First(d => d.Name == projectName && d.Path == projectPath);
            SelectedProject = project;
            LoadProject();
        }

        public void ImportCsv()
        {
            _csvImportExport.ImportCsv(ArchiveItems);
        }

        public void ExportCsv()
        {
            _csvImportExport.ExportCsv(SelectedProject, ArchiveItems, SelectedLanguage);
        }


        private void LoadProject()
        {
            Languages.Clear();
            Languages.AddRange(Directory.GetDirectories(SelectedProject.Path).Select(Path.GetFileName));
            if (Languages.Contains("en"))
            {
                SelectedLanguage = "en";
            }
            else
            {
                SelectedLanguage = Languages[0];
            }
        }

        private void SaveProjects()
        {
            File.WriteAllText(_projectPath, JsonConvert.SerializeObject(Projects.ToArray()));
        }
    }
}