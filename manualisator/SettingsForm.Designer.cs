namespace manualisator
{
    partial class SettingsForm
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
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbBaseDirectory = new System.Windows.Forms.TextBox();
            this.tbTemplateFilename_DE = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTemplateFilename_EN = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lbWarning = new System.Windows.Forms.Label();
            this.tbTemplatesDirectory = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbManualsDirectory = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbFilesDirectory = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilenameTemplate = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btOK
            // 
            this.btOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOK.Location = new System.Drawing.Point(674, 556);
            this.btOK.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(216, 72);
            this.btOK.TabIndex = 3;
            this.btOK.Text = "&OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(900, 556);
            this.btCancel.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(216, 72);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "&Abbrechen";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Arbeitsverzeichnis";
            // 
            // tbBaseDirectory
            // 
            this.tbBaseDirectory.Location = new System.Drawing.Point(265, 23);
            this.tbBaseDirectory.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbBaseDirectory.Name = "tbBaseDirectory";
            this.tbBaseDirectory.Size = new System.Drawing.Size(783, 31);
            this.tbBaseDirectory.TabIndex = 5;
            // 
            // tbTemplateFilename_DE
            // 
            this.tbTemplateFilename_DE.Location = new System.Drawing.Point(265, 73);
            this.tbTemplateFilename_DE.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbTemplateFilename_DE.Name = "tbTemplateFilename_DE";
            this.tbTemplateFilename_DE.Size = new System.Drawing.Size(848, 31);
            this.tbTemplateFilename_DE.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(235, 25);
            this.label2.TabIndex = 7;
            this.label2.Text = "Dokumentvorlage Deutsch";
            // 
            // tbTemplateFilename_EN
            // 
            this.tbTemplateFilename_EN.Location = new System.Drawing.Point(265, 123);
            this.tbTemplateFilename_EN.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbTemplateFilename_EN.Name = "tbTemplateFilename_EN";
            this.tbTemplateFilename_EN.Size = new System.Drawing.Size(848, 31);
            this.tbTemplateFilename_EN.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 128);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(237, 25);
            this.label3.TabIndex = 9;
            this.label3.Text = "Dokumentvorlage Englisch";
            // 
            // lbWarning
            // 
            this.lbWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbWarning.AutoSize = true;
            this.lbWarning.ForeColor = System.Drawing.Color.Red;
            this.lbWarning.Location = new System.Drawing.Point(26, 578);
            this.lbWarning.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lbWarning.Name = "lbWarning";
            this.lbWarning.Size = new System.Drawing.Size(260, 25);
            this.lbWarning.TabIndex = 11;
            this.lbWarning.Text = "WARNUNGSTEXT STEHT HIER";
            // 
            // tbTemplatesDirectory
            // 
            this.tbTemplatesDirectory.Location = new System.Drawing.Point(265, 173);
            this.tbTemplatesDirectory.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbTemplatesDirectory.Name = "tbTemplatesDirectory";
            this.tbTemplatesDirectory.Size = new System.Drawing.Size(848, 31);
            this.tbTemplatesDirectory.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 178);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 25);
            this.label4.TabIndex = 12;
            this.label4.Text = "Vorlagen";
            // 
            // tbManualsDirectory
            // 
            this.tbManualsDirectory.Location = new System.Drawing.Point(265, 223);
            this.tbManualsDirectory.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbManualsDirectory.Name = "tbManualsDirectory";
            this.tbManualsDirectory.Size = new System.Drawing.Size(848, 31);
            this.tbManualsDirectory.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 228);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 25);
            this.label5.TabIndex = 14;
            this.label5.Text = "Handbücher";
            // 
            // tbFilesDirectory
            // 
            this.tbFilesDirectory.Location = new System.Drawing.Point(265, 273);
            this.tbFilesDirectory.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbFilesDirectory.Name = "tbFilesDirectory";
            this.tbFilesDirectory.Size = new System.Drawing.Size(848, 31);
            this.tbFilesDirectory.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 278);
            this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 25);
            this.label6.TabIndex = 16;
            this.label6.Text = "Dateien";
            // 
            // tbFilenameTemplate
            // 
            this.tbFilenameTemplate.Location = new System.Drawing.Point(265, 328);
            this.tbFilenameTemplate.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tbFilenameTemplate.Name = "tbFilenameTemplate";
            this.tbFilenameTemplate.Size = new System.Drawing.Size(848, 31);
            this.tbFilenameTemplate.TabIndex = 23;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(26, 333);
            this.label9.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(177, 25);
            this.label9.TabIndex = 22;
            this.label9.Text = "Dateinamenvorlage";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(265, 371);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(389, 29);
            this.checkBox1.TabIndex = 24;
            this.checkBox1.Text = "Sprachspezifische Lesezeichen verwenden";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1061, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(54, 31);
            this.button1.TabIndex = 25;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(1136, 650);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.tbFilenameTemplate);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbFilesDirectory);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbManualsDirectory);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbTemplatesDirectory);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbWarning);
            this.Controls.Add(this.tbTemplateFilename_EN);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbTemplateFilename_DE);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbBaseDirectory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "SettingsForm";
            this.Text = "Einstellungen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbBaseDirectory;
        private System.Windows.Forms.TextBox tbTemplateFilename_DE;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbTemplateFilename_EN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbWarning;
        private System.Windows.Forms.TextBox tbTemplatesDirectory;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbManualsDirectory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbFilesDirectory;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilenameTemplate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
    }
}