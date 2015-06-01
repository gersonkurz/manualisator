namespace manualisator
{
    partial class WarnBeforeOverwriteForm
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
            this.btRename = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.lbMessage = new System.Windows.Forms.Label();
            this.btOverwriteAlways = new System.Windows.Forms.Button();
            this.btOverwrite = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btRename
            // 
            this.btRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btRename.Location = new System.Drawing.Point(14, 230);
            this.btRename.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btRename.Name = "btRename";
            this.btRename.Size = new System.Drawing.Size(203, 45);
            this.btRename.TabIndex = 5;
            this.btRename.Text = "&Umbenennen";
            this.btRename.UseVisualStyleBackColor = true;
            this.btRename.Click += new System.EventHandler(this.btRename_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(652, 230);
            this.btCancel.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(203, 45);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "&Abbrechen";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // lbMessage
            // 
            this.lbMessage.AutoSize = true;
            this.lbMessage.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMessage.ForeColor = System.Drawing.Color.Red;
            this.lbMessage.Location = new System.Drawing.Point(12, 20);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(618, 32);
            this.lbMessage.TabIndex = 6;
            this.lbMessage.Text = "Achtung: Die Datei \'blablabla_blablabla\' existiert bereits. ";
            // 
            // btOverwriteAlways
            // 
            this.btOverwriteAlways.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOverwriteAlways.Location = new System.Drawing.Point(439, 230);
            this.btOverwriteAlways.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btOverwriteAlways.Name = "btOverwriteAlways";
            this.btOverwriteAlways.Size = new System.Drawing.Size(203, 45);
            this.btOverwriteAlways.TabIndex = 7;
            this.btOverwriteAlways.Text = "&Immer überschreiben";
            this.btOverwriteAlways.UseVisualStyleBackColor = true;
            this.btOverwriteAlways.Click += new System.EventHandler(this.btOverwriteAlways_Click);
            // 
            // btOverwrite
            // 
            this.btOverwrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOverwrite.Location = new System.Drawing.Point(226, 230);
            this.btOverwrite.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btOverwrite.Name = "btOverwrite";
            this.btOverwrite.Size = new System.Drawing.Size(203, 45);
            this.btOverwrite.TabIndex = 8;
            this.btOverwrite.Text = "&Überschreiben";
            this.btOverwrite.UseVisualStyleBackColor = true;
            this.btOverwrite.Click += new System.EventHandler(this.btOverwrite_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 160);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(843, 31);
            this.textBox1.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "Neuer Dateiname:";
            // 
            // WarnBeforeOverwriteForm
            // 
            this.AcceptButton = this.btRename;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(861, 290);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btOverwrite);
            this.Controls.Add(this.btOverwriteAlways);
            this.Controls.Add(this.lbMessage);
            this.Controls.Add(this.btRename);
            this.Controls.Add(this.btCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 10.8F);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "WarnBeforeOverwriteForm";
            this.Text = "Warnung";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btRename;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.Button btOverwriteAlways;
        private System.Windows.Forms.Button btOverwrite;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
    }
}