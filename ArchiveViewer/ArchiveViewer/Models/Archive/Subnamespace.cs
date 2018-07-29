using System.Collections.Generic;

namespace ArchiveViewer.Models.Archive
{
    public class Subnamespace
    {
        public string Namespace { get; set; }
        public List<Child> Children { get; set; }
        public List<Subnamespace> Subnamespaces { get; set; }

        public override string ToString()
        {
            return Namespace;
        }
    }
}