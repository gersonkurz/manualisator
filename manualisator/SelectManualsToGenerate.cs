using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using manualisator.DBSchema;

namespace manualisator
{
    public partial class SelectManualsToGenerate : Form
    {
        private readonly List<Manual> ManualsToGenerate;

        public SelectManualsToGenerate(Dictionary<long, Manual> manualByID, List<Manual> manualsToGenerate)
        {
            InitializeComponent();
            ManualsToGenerate = manualsToGenerate;

            // need to sort the list alphabetically
            foreach (Manual m in manualByID.Values.OrderBy(m => m.Device))
            {
                var item = listView1.Items.Add(m.Device);
                item.SubItems.Add(m.Language);
                item.SubItems.Add(m.Title1);
                item.SubItems.Add(m.Title2);
                item.Tag = m;
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
                ManualsToGenerate.Add(item.Tag as Manual);
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
