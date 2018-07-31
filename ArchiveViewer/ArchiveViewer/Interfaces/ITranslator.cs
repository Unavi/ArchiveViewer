namespace ArchiveViewer.Interfaces
{
    public interface ITranslator
    {
        int MaxTranslations();
        string Translate(string text, string culture);
        string[] Translate(string[] texts, string culture);
        void Init();
    }
}