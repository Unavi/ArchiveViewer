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

        public void Init()
        {
            string token = File.ReadAllText("ArchiveViewer.json");
            GoogleCredential googleCredential = GoogleCredential.FromJson(token);
            _translationClient = TranslationClient.Create(googleCredential);
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