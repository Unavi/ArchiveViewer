using Caliburn.Micro;

namespace ArchiveViewer.Models
{
    public class ArchiveItem : PropertyChangedBase
    {
        private string _key;
        private string _namespace;

        private string _native;

        private string _translated;

        public ArchiveItem(string @namespace, string key, string native, string translated)
        {
            _key = key;
            _namespace = @namespace;
            _native = native;
            _translated = translated;
        }

        public string Namespace
        {
            get => _namespace;
            set
            {
                if (_namespace == value) return;
                _namespace = value;
                NotifyOfPropertyChange();
            }
        }

        public string Key
        {
            get => _key;
            set
            {
                if (_key == value) return;
                _key = value;
                NotifyOfPropertyChange();
            }
        }

        public string Native
        {
            get => _native;
            set
            {
                if (_native == value) return;
                _native = value;
                NotifyOfPropertyChange();
            }
        }

        public string Translated
        {
            get => _translated;
            set
            {
                if (_translated == value) return;
                _translated = value;
                NotifyOfPropertyChange();
            }
        }
    }
}