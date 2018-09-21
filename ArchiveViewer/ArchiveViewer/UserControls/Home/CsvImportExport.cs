using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ArchiveViewer.Models;
using Caliburn.Micro;
using CsvHelper;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ArchiveViewer.UserControls.Home
{
    public class CsvImportExport
    {
        public void ImportCsv(BindableCollection<ArchiveItem> archiveItems)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Csv documents (.csv)|*.csv";

                bool? result = dlg.ShowDialog();
                if (result == false)
                {
                    return;
                }

                using (TextReader reader = new StreamReader(dlg.FileName))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.RegisterClassMap<ArchiveItemMap>();
                    csv.Configuration.Encoding = Encoding.UTF8;

                    archiveItems.Clear();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var record = new ArchiveItem(csv[0], csv[1], csv[2], csv[3]);
                        archiveItems.Add(record);
                    }

                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }

        public void ExportCsv(Project project, IList<ArchiveItem> archiveItems, string selectedLanguage)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = project.Name + "_" + selectedLanguage;
                dlg.DefaultExt = ".csv";
                dlg.Filter = "Csv documents (.csv)|*.csv";

                bool? result = dlg.ShowDialog();
                if (result == false)
                {
                    return;
                }

                using (TextWriter writer = new StreamWriter(dlg.FileName))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<ArchiveItemMap>();
                    csv.Configuration.Encoding = Encoding.UTF8;
                    csv.WriteRecords(archiveItems);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }
    }
}