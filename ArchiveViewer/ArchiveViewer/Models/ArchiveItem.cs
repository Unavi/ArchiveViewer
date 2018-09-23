using System;
using Caliburn.Micro;

namespace ArchiveViewer.Models
{
    public class ArchiveItem : PropertyChangedBase
    {
        private bool _hasChanges;
        private string _key;
        private string _namespace;
        private string _native;

        private string _reverseTranslated;
        private string _translated;

        private string _translatedOriginal;

        public ArchiveItem(string @namespace, string key, string native, string translated)
        {
            _key = key;
            _namespace = @namespace;
            _native = native;
            _translated = translated;
            _translatedOriginal = _translated;
        }

        public string ReverseTranslated
        {
            get => _reverseTranslated;
            set
            {
                if (_reverseTranslated == value) return;
                _reverseTranslated = value;
                NotifyOfPropertyChange();
            }
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

        public string NativeWithChangeMarker => HasChanges ? "*" + _native : _native;

        public string Translated
        {
            get => _translated;
            set
            {
                if (_translated == value) return;
                _translated = value;
                NotifyOfPropertyChange();
                if (_translated != _translatedOriginal)
                {
                    HasChanges = true;
                    TranslatedChanged?.Invoke(this, true);
                    NotifyOfPropertyChange(() => NativeWithChangeMarker);
                }
                else
                {
                    HasChanges = false;
                    TranslatedChanged?.Invoke(this, false);
                    NotifyOfPropertyChange(() => NativeWithChangeMarker);
                }
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
            }
        }

        public event EventHandler<bool> TranslatedChanged;

        public void Reset()
        {
            _translated = _translatedOriginal;
            HasChanges = false;
            NotifyOfPropertyChange(() => NativeWithChangeMarker);
        }

        public void Persist()
        {
            _translatedOriginal = _translated;
            HasChanges = false;
            NotifyOfPropertyChange(() => NativeWithChangeMarker);
        }
    }
}