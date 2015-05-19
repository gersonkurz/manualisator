using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace manualisator
{
    public partial class ProgressDialog : Form, manualisator.Core.IDisplayCallback
    {
        private delegate void UpdateDisplay(string msg);

        private readonly manualisator.Core.LongRunningTask ActionToPerform;
        private bool HasBeenAbortedFlag;
        private int Errors = 0;
        private int Warnings = 0;
        private DateTime Start;
        private bool IsRunning = false;
        private readonly MainForm MainForm;

        public ProgressDialog(MainForm mainForm, manualisator.Core.LongRunningTask actionToPerform, string title)
        {
            InitializeComponent();
            ActionToPerform = actionToPerform;
            HasBeenAbortedFlag = false;
            Text = title;
            MainForm = mainForm;
            LbText.Text = "";
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            lock(ActionToPerform)
            {
                Start = DateTime.Now;
                IsRunning = true;
            }
            try
            {
                if (ActionToPerform.Initialize(this))
                {
                    ActionToPerform.Run();
                }
            }
            catch(Exception exception)
            {
                ActionToPerform.DumpException(exception, "ActionToPerform failed");
            }
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            progressBar1.Visible = true;
            backgroundWorker1.RunWorkerAsync();
            timer1.Interval = 1000;
            timer1.Start();
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

        public void AddInformation(string msg)
        {
            this.Invoke(new UpdateDisplay(UpdateInformation), msg);
        }

        public void AddWarning(string msg)
        {
            ++Warnings;
            this.Invoke(new UpdateDisplay(UpdateWarning), msg);
        }

        public void AddError(string msg)
        {
            ++Errors;
            this.Invoke(new UpdateDisplay(UpdateError), msg);
        }

        public void UpdateInformation(string msg)
        {
            LbText.ForeColor = Color.Black;
            MainForm.AddInformation(msg);
            LbText.Text = SkipSpecialChars(msg);
        }

        private string SkipSpecialChars(string msg)
        {
            if (msg.StartsWith("^") || msg.StartsWith("§"))
                return msg.Substring(1);
            return msg;
        }

        private void UpdateError(string msg)
        {
            LbErrors.Text = string.Format("Fehler: {0}", Errors);
            LbText.Text = SkipSpecialChars(msg);
            LbText.ForeColor = Color.Red;
            MainForm.AddError(msg);
        }

        private void UpdateWarning(string msg)
        {
            LbWarnings.Text = string.Format("Warnungen: {0}", Warnings);
            LbText.Text = SkipSpecialChars(msg);
            LbText.ForeColor = Color.DarkRed;
            MainForm.AddWarning(msg);
        }

        public void HasBeenAborted()
        {
            HasBeenAbortedFlag = true;
        }

        private void BtCancel_Click(object sender, EventArgs e)
        {
            ActionToPerform.SetCancelFlag();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool isRunning = false;
            lock(ActionToPerform)
            {
                isRunning = IsRunning;
            }
            if(isRunning)
            {
                LbElapsed.Text = string.Format("Dauer: {0}", DateTime.Now - Start);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (HasBeenAbortedFlag)
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            else
                this.DialogResult = System.Windows.Forms.DialogResult.OK;

            Close();
        }
    }
}
