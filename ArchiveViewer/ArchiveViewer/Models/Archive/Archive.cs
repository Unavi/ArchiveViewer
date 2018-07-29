using System.Collections.Generic;

namespace ArchiveViewer.Models.Archive
{
    public class Archive
    {
        public int FormatVersion { get; set; }
        public string Namespace { get; set; }
        public List<Child> Children { get; set; }
        public List<Subnamespace> Subnamespaces { get; set; }
    }
}