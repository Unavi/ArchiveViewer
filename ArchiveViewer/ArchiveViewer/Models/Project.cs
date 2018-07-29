using Caliburn.Micro;
using Newtonsoft.Json;

namespace ArchiveViewer.Models
{
    public class Project : PropertyChangedBase
    {
        private string _name;

        private string _path;

        [JsonProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyOfPropertyChange();
            }
        }

        [JsonProperty]
        public string Path
        {
            get => _path;
            set
            {
                if (_path == value) return;
                _path = value;
                NotifyOfPropertyChange();
            }
        }

        public Project(string name, string path)
        {
            _name = name;
            _path = path;
        }
    }
}