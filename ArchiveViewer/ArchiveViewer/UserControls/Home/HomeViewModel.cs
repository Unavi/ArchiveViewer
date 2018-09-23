using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using ArchiveViewer.Interfaces;
using ArchiveViewer.Library.Translation;
using ArchiveViewer.Models;
using Caliburn.Micro;
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

        private bool _hasUnsavedChanges;
        private BindableCollection<string> _languages;
        private string _projectPath = "projects.json";

        private BindableCollection<Project> _projects;

        private int _selectedArchiveItemCount;

        private BindableCollection<ArchiveItem> _selectedArchiveItems;

        private string _selectedLanguage;
        private Project _selectedProject;
        private ITranslator _translator;

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
            _translator = new GoogleTranslator();
            _translator.Init();
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
                        foreach (ArchiveItem archiveItem in ArchiveItems)
                        {
                            archiveItem.TranslatedChanged += ArchiveItemOnTranslatedChanged;
                        }
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
                NotifyOfPropertyChange(() => CanRevertSave);
                NotifyOfPropertyChange(() => CanSaveSelectedProject);
                NotifyOfPropertyChange(() => CanExportCsv);
                NotifyOfPropertyChange(() => CanImportCsv);
            }
        }

        public bool CanSaveChanges =>
            SelectedProject != null && !string.IsNullOrEmpty(SelectedLanguage) && _hasUnsavedChanges;

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
                NotifyOfPropertyChange(() => CanAutoTranslateSelected);
                NotifyOfPropertyChange(() => CanReverseTranslateSelected);
            }
        }

        public bool CanAutoTranslateSelected => _translator.IsInitialized() && SelectedArchiveItemCount != 0;
        public bool CanReverseTranslateSelected => _translator.IsInitialized() && SelectedArchiveItemCount != 0;

        public bool CanRevertSave => SelectedProject != null && _hasUnsavedChanges;

        private void ArchiveItemOnTranslatedChanged(object sender, bool hasChanges)
        {
            _hasUnsavedChanges = true;
            if (!hasChanges && !ArchiveItems.Any(d => d.HasChanges))
            {
                _hasUnsavedChanges = false;
            }
            NotifyOfPropertyChange(() => CanSaveChanges);
            NotifyOfPropertyChange(() => CanRevertSave);
        }

        public void SelectedArchiveItemsChanged()
        {
            SelectedArchiveItemCount = SelectedArchiveItems.Count;
        }

        public void AutoTranslateSelected()
        {
            string[] toTranslate = SelectedArchiveItems.Select(d => d.Native).ToArray();

            if (!_translator.IsInitialized())
            {
                MessageBox.Show("Translator is not initialized.", "Error", MessageBoxButtons.OK);
                return;
            }

            if (toTranslate.Length == 0)
            {
                MessageBox.Show("Nothing selected", "Error", MessageBoxButtons.OK);
                return;
            }

            if (toTranslate.Length > _translator.MaxTranslations())
            {
                MessageBox.Show("Can't translate more than 125 entries with one call.", "Error", MessageBoxButtons.OK);
                return;
            }

            try
            {
                IList<string> translatedLines = _translator.Translate(toTranslate, SelectedLanguage);
                for (var i = 0; i < translatedLines.Count; i++)
                {
                    string result = translatedLines[i];
                    SelectedArchiveItems[i].Translated = result;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }

        public void ReverseTranslateSelected()
        {
            string[] toTranslate = SelectedArchiveItems.Select(d => d.Translated).ToArray();

            if (!_translator.IsInitialized())
            {
                MessageBox.Show("Translator is not initialized.", "Error", MessageBoxButtons.OK);
                return;
            }

            if (toTranslate.Length == 0)
            {
                MessageBox.Show("Nothing selected", "Error", MessageBoxButtons.OK);
                return;
            }

            if (toTranslate.Length > _translator.MaxTranslations())
            {
                MessageBox.Show("Can't translate more than 125 entries with one call.", "Error", MessageBoxButtons.OK);
                return;
            }

            try
            {
                IList<string> translatedLines = _translator.Translate(toTranslate, "en");
                for (var i = 0; i < translatedLines.Count; i++)
                {
                    string result = translatedLines[i];
                    SelectedArchiveItems[i].ReverseTranslated = result;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }

        public void RevertSave()
        {
            _hasUnsavedChanges = false;
            NotifyOfPropertyChange(() => CanSaveChanges);
            NotifyOfPropertyChange(() => CanRevertSave);
            var tempLanguage = SelectedLanguage;
            _archiveParser.Revert(SelectedProject, SelectedLanguage, ArchiveItems);
            SelectProject(SelectedProject.Name, SelectedProject.Path);
            SelectedLanguage = tempLanguage;
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
                if (archiveItem.Namespace.ToLower().Contains(Filter.ToLower()))
                {
                    filterEventArgs.Accepted = true;
                    return;
                }

                if (archiveItem.Key.ToLower().Contains(Filter.ToLower()))
                {
                    filterEventArgs.Accepted = true;
                    return;
                }

                if (archiveItem.NativeWithChangeMarker.ToLower().Contains(Filter.ToLower()))
                {
                    filterEventArgs.Accepted = true;
                    return;
                }

                if (archiveItem.Translated.ToLower().Contains(Filter.ToLower()))
                {
                    filterEventArgs.Accepted = true;
                }
            }
        }

        public void SaveChanges()
        {
            _hasUnsavedChanges = false;
            NotifyOfPropertyChange(() => CanSaveChanges);
            NotifyOfPropertyChange(() => CanRevertSave);
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