using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace manualisator
{
    public partial class FindTextForm : Form
    {
        public string FindThisText;

        public FindTextForm()
        {
            InitializeComponent();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            FindThisText = tbFindText.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
