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
        public WarnBeforeOverwriteForm(string filename)
        {
            InitializeComponent();
            lbMessage.Text = string.Format("ACHTUNG: Die Datei '{0}' existiert bereits.", Path.GetFileName(filename));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
