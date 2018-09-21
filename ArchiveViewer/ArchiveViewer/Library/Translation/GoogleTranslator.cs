using System.Collections.Generic;
using System.IO;
using ArchiveViewer.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;

namespace ArchiveViewer.Library.Translation
{
    public class GoogleTranslator : Singleton<GoogleTranslator>, ITranslator
    {
        private TranslationClient _translationClient;
        private string googleKey = "GoogleKey.json";
        private bool _isInitialized;

        public void Init()
        {
            if (File.Exists(googleKey))
            {
                string token = File.ReadAllText(googleKey);
                GoogleCredential googleCredential = GoogleCredential.FromJson(token);
                _translationClient = TranslationClient.Create(googleCredential);
                _isInitialized = true;
            }
        }

        public bool IsInitialized()
        {
            return _isInitialized;
        }

        public int MaxTranslations()
        {
            return 125;
        }

        public string Translate(string text, string culture)
        {
            var result = _translationClient.TranslateText(text, culture);
            return result.TranslatedText;
        }

        public string[] Translate(string[] texts, string culture)
        {
            List<string> translations = new List<string>();
            var result = _translationClient.TranslateText(texts, culture);
            foreach (var translationResult in result)
            {
                translations.Add(translationResult.TranslatedText);
            }
            return translations.ToArray();
        }
    }
}