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
            checkBox1.Checked = IsTrue(Program.Settings.UseLanguageSpecificBookmarks);
        }

        private bool IsTrue(string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("1", StringComparison.OrdinalIgnoreCase);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(tbBaseDirectory.Text))
            {
                lbWarning.Text = "Fehler: Das Basisverzeichnis existiert nicht.";
                return;
            }
            Program.Settings["BaseDirectory"] = tbBaseDirectory.Text;
            Program.Settings["TemplateFilename_DE"] = tbTemplateFilename_DE.Text;
            Program.Settings["TemplateFilename_EN"] = tbTemplateFilename_EN.Text;
            Program.Settings["ManualsDirectory"] = tbManualsDirectory.Text;
            Program.Settings["TemplatesDirectory"] = tbTemplatesDirectory.Text;
            Program.Settings["FilesDirectory"] = tbFilesDirectory.Text;
            Program.Settings["FilenameTemplate"] = tbFilenameTemplate.Text;
            Program.Settings["UseLanguageSpecificBookmarks"] = checkBox1.Checked ? "true" : "false";
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
    }
}
