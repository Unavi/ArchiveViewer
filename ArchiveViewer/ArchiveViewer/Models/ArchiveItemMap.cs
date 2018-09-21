using CsvHelper.Configuration;

namespace ArchiveViewer.Models
{
    public sealed class ArchiveItemMap : CsvClassMap<ArchiveItem>
    {
        public ArchiveItemMap()
        {
            Map(m => m.Namespace).Index(0).Name("Namespace");
            Map(m => m.Key).Index(1).Name("Key");
            Map(m => m.Native).Index(2).Name("Native");
            Map(m => m.Translated).Index(3).Name("Translated");
        }
    }
}