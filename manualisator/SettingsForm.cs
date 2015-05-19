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

namespace manualisator
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            lbWarning.Text = "";
            tbBaseDirectory.Text = Program.Settings.BaseDirectory;
            tbTemplateFilename_DE.Text = Program.Settings.TemplateFilename_DE;
            tbTemplateFilename_EN.Text = Program.Settings.TemplateFilename_EN;
            tbManualsDirectory.Text = Program.Settings.ManualsDirectory;
            tbTemplatesDirectory.Text = Program.Settings.TemplatesDirectory;
            tbFilesDirectory.Text = Program.Settings.FilesDirectory;
            tbFilenameTemplate.Text = Program.Settings.FilenameTemplate;
            checkBox1.Checked = Program.Settings.UseBookmarksFromExcelSheet;
            checkBox2.Checked = Program.Settings.UseFilenameTemplate;
            tbFilenameTemplate.Enabled = checkBox2.Checked;
            checkBox3.Checked = Program.Settings.CreateDocumentSortOrderFromBookmarks;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            lbWarning.Text = "";
            if (!Directory.Exists(tbBaseDirectory.Text))
            {
                lbWarning.Text = "Fehler: Das Basisverzeichnis existiert nicht.";
                return;
            }

            // check if the directories exist
            string[] folders = tbFilesDirectory.Text.Split(';', ',');
            string pathName, baseDirectory = tbBaseDirectory.Text;

            foreach (string folderName in folders)
            {
                pathName = Path.Combine(baseDirectory, folderName);
                if (!Directory.Exists(pathName))
                {
                    lbWarning.Text = string.Format("Fehler: Das Dateien-Verzeichnis {0} existiert nicht.", pathName);
                    return;
                }
            }

            pathName = Path.Combine(baseDirectory, tbManualsDirectory.Text);
            if( !Directory.Exists(pathName) )
            {
                lbWarning.Text = string.Format("Fehler: Das Handbücher-Verzeichnis {0} existiert nicht.", pathName);
                return;
            }

            pathName = Path.Combine(baseDirectory, tbTemplatesDirectory.Text);
            if (!Directory.Exists(pathName))
            {
                lbWarning.Text = string.Format("Fehler: Das Templates-Verzeichnis {0} existiert nicht.", pathName);
                return;
            }

            Program.Settings["BaseDirectory"] = tbBaseDirectory.Text;
            Program.Settings["TemplateFilename_DE"] = tbTemplateFilename_DE.Text;
            Program.Settings["TemplateFilename_EN"] = tbTemplateFilename_EN.Text;
            Program.Settings["ManualsDirectory"] = tbManualsDirectory.Text;
            Program.Settings["TemplatesDirectory"] = tbTemplatesDirectory.Text;
            Program.Settings["FilesDirectory"] = tbFilesDirectory.Text;
            Program.Settings["FilenameTemplate"] = tbFilenameTemplate.Text;
            Program.Settings["UseBookmarksFromExcelSheet"] = checkBox1.Checked ? true : false;
            Program.Settings["UseFilenameTemplate"] = checkBox2.Checked ? true : false;
            Program.Settings["CreateDocumentSortOrderFromBookmarks"] = checkBox3.Checked ? true : false;            
            Program.PersistentSettings.Save();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = tbBaseDirectory.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbBaseDirectory.Text = dialog.SelectedPath;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            tbFilenameTemplate.Enabled = checkBox2.Checked;
        }
    }
}
