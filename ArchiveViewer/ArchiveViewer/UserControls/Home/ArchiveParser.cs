using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using ArchiveViewer.Models;
using ArchiveViewer.Models.Archive;
using Newtonsoft.Json;

namespace ArchiveViewer.UserControls.Home
{
    public class ArchiveParser
    {
        public IEnumerable<ArchiveItem> ParseArchive(Project project, string language)
        {
            var archiveItems = new List<ArchiveItem>();
            string path = project.Path + "\\" + language;
            path += "\\" + Path.GetFileName(project.Path) + ".archive";

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "Looks like the selected folder is not a proper language folder of unreal.\n\nThe folder is usually located at:\n[YourGame]\\Content\\Localization\\[Target]\n\nFor example:\nFirstPersonShooter\\Content\\Localization\\Game",
                    "Error");
                throw new InvalidArchiveFolderException();
            }

            var tree = JsonConvert.DeserializeObject<Archive>(File.ReadAllText(path));

            foreach (var child in tree.Children)
            {
                archiveItems.Add(new ArchiveItem(tree.Namespace, child.Key, child.Source.Text, child.Translation.Text));
            }

            ReadSubnamespaces(tree.Subnamespaces, archiveItems, "");
            archiveItems = archiveItems.OrderBy(d => d.Namespace).ThenBy(d => d.Key).ToList();

            return archiveItems;
        }

        private void ReadSubnamespaces(List<Subnamespace> subnamespaces, IList<ArchiveItem> archiveItems,
            string parentNamespace)
        {
            foreach (var subnamespace in subnamespaces)
            {
                if (subnamespace.Children != null)
                {
                    foreach (var child in subnamespace.Children)
                    {
                        archiveItems.Add(new ArchiveItem(
                            (parentNamespace + "." + subnamespace.Namespace).TrimStart('.'), child.Key,
                            child.Source.Text,
                            child.Translation.Text));
                    }
                }

                if (subnamespace.Subnamespaces != null)
                {
                    ReadSubnamespaces(subnamespace.Subnamespaces, archiveItems,
                        parentNamespace + "." + subnamespace.Namespace);
                }
            }
        }

        private void WriteSubnamespaces(IGrouping<string, ArchiveItem> grouping, Archive archive)
        {
            var subnamespaces = FindSubnamespaceForNamespace(grouping.Key,
                archive.Subnamespaces ?? (archive.Subnamespaces = new List<Subnamespace>()));

            var children = grouping.Select(d => new Child
            {
                Key = d.Key,
                Source = new Source
                {
                    Text = d.Native
                },
                Translation = new Translation
                {
                    Text = d.Translated
                }
            });

            subnamespaces.Add(new Subnamespace
            {
                Namespace = grouping.Key.Split('.')[grouping.Key.Split('.').Length - 1],
                Children = children.ToList()
            });
        }

        private List<Subnamespace> FindSubnamespaceForNamespace(string @namespace,
            List<Subnamespace> parentSubnamespaces)
        {
            string[] namespaceParts = @namespace.Split('.');
            if (namespaceParts.Length == 1)
            {
                return parentSubnamespaces;
            }

            if (namespaceParts.Length > 1)
            {
                if (parentSubnamespaces.FirstOrDefault(d => d.Namespace == namespaceParts[0]) == null)
                {
                    parentSubnamespaces.Add(new Subnamespace
                    {
                        Namespace = namespaceParts[0]
                    });
                }

                var subnamespaces = parentSubnamespaces.Where(d => d.Namespace == namespaceParts[0]);
                foreach (var subnamespace in subnamespaces)
                {
                    var fittingSubnameSpace = FindSubnamespaceForNamespace(
                        @namespace.Replace(namespaceParts[0] + ".", ""),
                        subnamespace.Subnamespaces ?? (subnamespace.Subnamespaces = new List<Subnamespace>()));
                    if (fittingSubnameSpace != null)
                    {
                        return fittingSubnameSpace;
                    }
                }
            }

            throw new Exception("Could not find parent namespace. Something is wrong.");
        }

        public void SaveArchive(Project project, string language, IEnumerable<ArchiveItem> archiveItems)
        {
            var archive = new Archive();
            archive.FormatVersion = 2;
            int namespaceDepth = 0;
            foreach (var grouping in archiveItems.GroupBy(d => d.Namespace).OrderBy(d => d.Key))
            {
                if (grouping.Key == "")
                {
                    var children = grouping.Select(d => new Child
                    {
                        Key = d.Key,
                        Source = new Source
                        {
                            Text = d.Native
                        },
                        Translation = new Translation
                        {
                            Text = d.Translated
                        }
                    });
                    archive.Namespace = "";
                    archive.Children = children.ToList();
                }
                else
                {
                    WriteSubnamespaces(grouping, archive);
                }

                namespaceDepth = Math.Max(namespaceDepth, grouping.Key.Split('.').Length);
            }

            var serializedArchive = JsonConvert.SerializeObject(archive, Formatting.Indented);

            //Change format to conform with unreal format
            serializedArchive = serializedArchive.Replace("  ", "\t");


            for (int i = 0; i <= namespaceDepth + 1; i++)
            {
                string tabs = "\t\t\t" + new string('\t', i * 2);
                serializedArchive = serializedArchive.Replace("\n" + tabs + "\"Source\": {",
                    "\n" + tabs + "\"Source\":\r\n" + tabs + "{");
                serializedArchive = serializedArchive.Replace("\n" + tabs + "\"Translation\": {",
                    "\n" + tabs + "\"Translation\":\r\n" + tabs + "{");
                serializedArchive = serializedArchive.Replace(",\r\n" + tabs + "\"Subnamespaces\": null", "");
                serializedArchive = serializedArchive.Replace("\r\n" + tabs + "\"Children\": null,", "");
            }

            string path = project.Path + "\\" + language;
            path += "\\" + Path.GetFileName(project.Path) + ".archive";
            File.Copy(path, path.Replace(".archive", ".bak"), true);
            File.WriteAllText(path, serializedArchive, Encoding.Unicode);
        }

        public void Revert(Project project, string language)
        {
            string path = project.Path + "\\" + language;
            path += "\\" + Path.GetFileName(project.Path) + ".bak";
            if (File.Exists(path.Replace(".bak", ".archive")))
            {
                File.Copy(path, path.Replace(".bak", ".archive"), true);
            }
        }
    }
}