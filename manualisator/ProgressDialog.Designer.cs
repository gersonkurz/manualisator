namespace manualisator
{
    partial class ProgressDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.BtCancel = new System.Windows.Forms.Button();
            this.LbText = new System.Windows.Forms.Label();
            this.LbElapsed = new System.Windows.Forms.Label();
            this.LbErrors = new System.Windows.Forms.Label();
            this.LbWarnings = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(29, 39);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1142, 78);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 0;
            // 
            // BtCancel
            // 
            this.BtCancel.Location = new System.Drawing.Point(470, 291);
            this.BtCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BtCancel.Name = "BtCancel";
            this.BtCancel.Size = new System.Drawing.Size(266, 37);
            this.BtCancel.TabIndex = 1;
            this.BtCancel.Text = "&Abbrechen";
            this.BtCancel.UseVisualStyleBackColor = true;
            this.BtCancel.Click += new System.EventHandler(this.BtCancel_Click);
            // 
            // LbText
            // 
            this.LbText.AutoSize = true;
            this.LbText.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbText.Location = new System.Drawing.Point(35, 134);
            this.LbText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LbText.Name = "LbText";
            this.LbText.Size = new System.Drawing.Size(492, 25);
            this.LbText.TabIndex = 2;
            this.LbText.Text = "BESCHREIBUNG DES AKTUELLEN VORGANGS STEHT HIER";
            // 
            // LbElapsed
            // 
            this.LbElapsed.AutoSize = true;
            this.LbElapsed.Location = new System.Drawing.Point(35, 236);
            this.LbElapsed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LbElapsed.Name = "LbElapsed";
            this.LbElapsed.Size = new System.Drawing.Size(215, 25);
            this.LbElapsed.TabIndex = 3;
            this.LbElapsed.Text = "Dauer: - Wird ermittelt -";
            // 
            // LbErrors
            // 
            this.LbErrors.AutoSize = true;
            this.LbErrors.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbErrors.Location = new System.Drawing.Point(605, 236);
            this.LbErrors.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LbErrors.Name = "LbErrors";
            this.LbErrors.Size = new System.Drawing.Size(151, 25);
            this.LbErrors.TabIndex = 4;
            this.LbErrors.Text = "Fehler: - keine -";
            // 
            // LbWarnings
            // 
            this.LbWarnings.AutoSize = true;
            this.LbWarnings.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbWarnings.Location = new System.Drawing.Point(914, 236);
            this.LbWarnings.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LbWarnings.Name = "LbWarnings";
            this.LbWarnings.Size = new System.Drawing.Size(203, 25);
            this.LbWarnings.TabIndex = 5;
            this.LbWarnings.Text = "Warnungen: - Keine -";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1206, 341);
            this.ControlBox = false;
            this.Controls.Add(this.LbWarnings);
            this.Controls.Add(this.LbErrors);
            this.Controls.Add(this.LbElapsed);
            this.Controls.Add(this.LbText);
            this.Controls.Add(this.BtCancel);
            this.Controls.Add(this.progressBar1);
            this.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.Text = "VORGANGSBESCHREIBUNG STEHT HIER";
            this.Load += new System.EventHandler(this.ProgressDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button BtCancel;
        private System.Windows.Forms.Label LbText;
        private System.Windows.Forms.Label LbElapsed;
        private System.Windows.Forms.Label LbErrors;
        private System.Windows.Forms.Label LbWarnings;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Timer timer1;
    }
}