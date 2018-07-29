namespace ArchiveViewer.Models.Archive
{
    public class Child
    {
        public Source Source { get; set; }
        public Translation Translation { get; set; }
        public string Key { get; set; }

        public override string ToString()
        {
            return Key + " " + Source + " " + Translation;
        }
    }
}