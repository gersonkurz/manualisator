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
    public partial class WarnBeforeOverwriteForm : Form
    {
        public string NewFilename;

        public WarnBeforeOverwriteForm(string filename)
        {
            InitializeComponent();
            lbMessage.Text = string.Format("Achtung: die Datei '{0}' existiert bereits.", Path.GetFileName(filename));
            NewFilename = filename;
            textBox1.Text = filename;
        }

        private void btRename_Click(object sender, EventArgs e)
        {
            NewFilename = textBox1.Text;
            if (File.Exists(NewFilename))
            {
                label1.Text = string.Format("Achtung: die Datei '{0}' existiert auch bereits.", Path.GetFileName(NewFilename));
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btOverwrite_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btOverwriteAlways_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Program.Settings["WarnBeforeOverwriting"] = false;
            Program.PersistentSettings.Save();
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
