using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using log4net;
using System.Reflection;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace manualisator
{
    public partial class MainForm : Form, manualisator.Core.IDisplayCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); 
        
        private System.Drawing.Font BoldFont = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        public MainForm()
        {
            InitializeComponent();      

            Assembly myAssembly = Assembly.GetExecutingAssembly();

            var v = myAssembly.GetName().Version;
            string version = string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Revision);
            
            AddInformation("§Willkommen bei MANUALISATOR Version {0}", version);
            AddInformation("Copyright (C) 2015 Gerson Kurz. All Rights Reserved.");
            AddInformation("");

            if (VerifyThatBaseDirectoryIsValid())
            {
                AddInformation(string.Format("Das Basis-Verzeichnis ist '{0}'.", Program.Settings.BaseDirectory));
            }

            this.Text = string.Format("manualisator {0}", version);
            UpdateMenuAvailability();
        }

        private bool VerifyThatBaseDirectoryIsValid()
        {
            if (!IsValidBaseDirectory)
            {
                AddError(string.Format("ACHTUNG: Das Basis-Verzeichnis '{0}' ungültig. Bitte 'Einstellungen' aufrufen und korrigieren.", Program.Settings.BaseDirectory));
                return false;
            }
            return true;
        }


        private bool IsValidBaseDirectory
        {
            get
            {
                return Directory.Exists(Program.Settings.BaseDirectory);
            }
        }
        
        private void programmendeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void vorlagenAktualisierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VerifyThatBaseDirectoryIsValid())
            {
                using (var task = new manualisator.Core.TemplatesUpdater())
                {
                    new ProgressDialog(this, task, "Bitte warten, die Vorlagen werden aktualisiert...").ShowDialog(this);
                }
            }
        }

        public void AddText(string msg, Color foreColor)
        {
            Log.InfoFormat(">> {0}", msg);
            foreach(string line in msg.Split('\n'))
            {
                string textForThisLine = line;
                System.Drawing.Font font = listView1.Font;
                ListViewItem lvi = null;

                if (textForThisLine.StartsWith("§"))
                {
                    textForThisLine = textForThisLine.Substring(1);
                    font = BoldFont;
                }
                if (textForThisLine.StartsWith("^"))
                {
                    textForThisLine = textForThisLine.Substring(1);
                    lvi = listView1.Items[listView1.Items.Count-1];
                    lvi.ForeColor = foreColor;
                    lvi.Font = font;
                    lvi.SubItems[0].Text = textForThisLine;
                }
                else
                {
                    lvi = new ListViewItem(textForThisLine);
                    lvi.ForeColor = foreColor;
                    lvi.Font = font;
                    listView1.Items.Add(lvi);
                }
                listView1.Items[listView1.Items.Count - 1].EnsureVisible();                
            }
        }

        public void AddInformation(string msg)
        {
            AddText(msg, Color.Black);
        }
        public void AddWarning(string msg)
        {
            AddText(msg, Color.DarkRed);
        }

        public void AddError(string msg)
        {
            AddText(msg, Color.Red);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            foreach(ListViewItem lvi in listView1.SelectedItems)
            {
                string temp = lvi.Text;
                int k = temp.IndexOf('\'');
                if( k > 0 )
                {
                    temp = temp.Substring(k + 1);
                    k = temp.IndexOf('\'');
                    if( k > 0)
                    {
                        temp = temp.Substring(0, k);
                        Log.WarnFormat("About to start file '{0}'", temp);
                        System.Diagnostics.Process.Start(temp);
                    }
                }
            }
        }

        private void handbücherErzeugenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VerifyThatBaseDirectoryIsValid())
            {
                using (var task = new manualisator.Core.ManualGenerator())
                {
                    if (task.PreInitialize(this))
                    {
                        var box = new SelectManualsToGenerate(task.ManualByID, task.ManualsToGenerate);
                        if (box.ShowDialog() == DialogResult.OK)
                        {
                            new ProgressDialog(this, task, "Bitte warten, die Handbücher werden erzeugt...").ShowDialog(this);
                        }
                    }
                }
            }
        }

        public void AddInformation(string msg, params object[] args)
        {
            AddInformation(string.Format(msg, args));
        }

        public void AddWarning(string msg, params object[] args)
        {
            AddWarning(string.Format(msg, args));
        }

        public void AddError(string msg, params object[] args)
        {
            AddError(string.Format(msg, args));
        }

        public void HasBeenAborted()
        {
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (VerifyThatBaseDirectoryIsValid())
            {
                using (var task = new manualisator.Core.RebuildLookupDocument())
                {
                    new ProgressDialog(this, task, "Bitte warten, der Cache wird aktualisiert...").ShowDialog(this);
                }
            }
        }

        private void einstellungenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldBaseDirectory = Program.Settings.BaseDirectory;
            if( new SettingsForm().ShowDialog() == DialogResult.OK )
            {
                UpdateMenuAvailability();
                if (VerifyThatBaseDirectoryIsValid())
                {
                    if (!oldBaseDirectory.Equals(Program.Settings.BaseDirectory))
                    {
                        AddInformation(string.Format("Das Basis-Verzeichnis ist jetzt '{0}'.", Program.Settings.BaseDirectory));
                    }
                }
            }
        }

        private void anzeigenWelcheTemplatesNICHTBenötigtWerdenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VerifyThatBaseDirectoryIsValid())
            {
                using (var task = new manualisator.Core.ShowDetailedManualInfo())
                {
                    if (task.PreInitialize(this))
                    {
                        var box = new SelectManualsToGenerate(task.ManualByID, task.ManualsToGenerate);
                        if (box.ShowDialog() == DialogResult.OK)
                        {
                            new ProgressDialog(this, task, "Bitte warten, die Information wird ermittelt...").ShowDialog(this);
                        }
                    }
                }
            }
        }

        private void suchenNachEinemTextInAllenVorlagenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VerifyThatBaseDirectoryIsValid())
            {
                var box = new FindTextForm();
                if (box.ShowDialog() == DialogResult.OK)
                {
                    using (var task = new manualisator.Core.FindTextContent(box.FindThisText))
                    {
                        new ProgressDialog(this, task, "Bitte warten, die Information wird ermittelt...").ShowDialog(this);
                    }
                }
            }
        }

        private void UpdateMenuAvailability()
        {
            bool enabled = IsValidBaseDirectory;
            anzeigenWelcheTemplatesNICHTBenötigtWerdenToolStripMenuItem.Enabled = enabled;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (VerifyThatBaseDirectoryIsValid())
            {
                var box = new SelectDirectManualsToGenerate();
                if (box.ShowDialog() == DialogResult.OK)
                {
                    using (var task = new manualisator.Core.CreateManualsDirectly(box.ManualsToGenerate))
                    {
                        string dialogCaption = (box.ManualsToGenerate.Count > 1) ?
                            "Bitte warten, die Handbücher werden erzeugt..." :
                            "Bitte warten, das Handbuch wird erzeugt...";

                        new ProgressDialog(this, task, dialogCaption).ShowDialog(this);
                    }
                }
            }
        }
    }
}
