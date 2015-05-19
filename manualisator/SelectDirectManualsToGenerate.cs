using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using manualisator.DBSchema;
using manualisator.Core;

namespace manualisator
{
    public partial class SelectDirectManualsToGenerate : Form
    {
        public readonly List<string> ManualsToGenerate = new List<string>();

        public SelectDirectManualsToGenerate()
        {
            InitializeComponent();

            foreach (string filename in Directory.GetFiles(Program.Settings.BaseDirectory))
            {
                if (filename.ToLower().EndsWith(".xls") || filename.ToLower().EndsWith(".xlsx"))
                {
                    string language = Strings.Language_DE;
                    if (filename.ToLower().Contains("_e_"))
                        language = Strings.Language_EN;

                    ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(filename));
                    lvi.SubItems.Add(language);
                    lvi.Tag = filename;
                    listView1.Items.Add(lvi);
                }
            }

            foreach (string base_directory in Directory.GetDirectories(Program.Settings.BaseDirectory))
            {
                if (Path.GetFileName(base_directory).ToUpper().StartsWith("C") ||
                    Path.GetFileName(base_directory).ToUpper().StartsWith("T"))
                {
                    foreach (string filename in Directory.GetFiles(base_directory))
                    {
                        if (filename.ToLower().EndsWith(".xls") || filename.ToLower().EndsWith(".xlsx"))
                        {
                            string language = Strings.Language_DE;
                            if (filename.ToLower().Contains("_e_"))
                                language = Strings.Language_EN;

                            ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(filename));
                            lvi.SubItems.Add(language);
                            lvi.Tag = filename;
                            listView1.Items.Add(lvi);
                        }
                    }
                }
            }

            
            btOK.Enabled = false;
            lbWarning.Text = "Sie müssen mindestens ein Handbuch auswählen.";
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            bool enabled = listView1.SelectedItems.Count > 0;

            lbWarning.Visible = !enabled;
            btOK.Enabled = enabled;

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem item in listView1.SelectedItems)
            {
                ManualsToGenerate.Add(item.Tag as string);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                btOK_Click(sender, e);
            }
        }
    }
}
