namespace MT_MDM
{
    partial class Ymodem
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
            this.btnXmodem128 = new System.Windows.Forms.Button();
            this.btnModem1K = new System.Windows.Forms.Button();
            this.btnYmodem = new System.Windows.Forms.Button();
            this.tbXmodem128 = new System.Windows.Forms.TextBox();
            this.tbXmodem1k = new System.Windows.Forms.TextBox();
            this.tbYmodem = new System.Windows.Forms.TextBox();
            this.rtbStatus = new System.Windows.Forms.RichTextBox();
            this.btnWorkingDir = new System.Windows.Forms.Button();
            this.tbWorkingDir = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnYmodemSend = new System.Windows.Forms.Button();
            this.btnModem1KSend = new System.Windows.Forms.Button();
            this.btnXmodem128Send = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnXmodem128
            // 
            this.btnXmodem128.Location = new System.Drawing.Point(48, 63);
            this.btnXmodem128.Name = "btnXmodem128";
            this.btnXmodem128.Size = new System.Drawing.Size(108, 23);
            this.btnXmodem128.TabIndex = 0;
            this.btnXmodem128.Text = "Xmodem 128 byte";
            this.btnXmodem128.UseVisualStyleBackColor = true;
            this.btnXmodem128.Click += new System.EventHandler(this.BtnXmodem128_Click);
            // 
            // btnModem1K
            // 
            this.btnModem1K.Location = new System.Drawing.Point(48, 101);
            this.btnModem1K.Name = "btnModem1K";
            this.btnModem1K.Size = new System.Drawing.Size(108, 23);
            this.btnModem1K.TabIndex = 1;
            this.btnModem1K.Text = "Xmodem 1k";
            this.btnModem1K.UseVisualStyleBackColor = true;
            this.btnModem1K.Click += new System.EventHandler(this.BtnModem1K_Click);
            // 
            // btnYmodem
            // 
            this.btnYmodem.Location = new System.Drawing.Point(48, 141);
            this.btnYmodem.Name = "btnYmodem";
            this.btnYmodem.Size = new System.Drawing.Size(108, 23);
            this.btnYmodem.TabIndex = 2;
            this.btnYmodem.Text = "Ymodem";
            this.btnYmodem.UseVisualStyleBackColor = true;
            this.btnYmodem.Click += new System.EventHandler(this.btnYmodem_Click);
            // 
            // tbXmodem128
            // 
            this.tbXmodem128.Location = new System.Drawing.Point(198, 63);
            this.tbXmodem128.Name = "tbXmodem128";
            this.tbXmodem128.Size = new System.Drawing.Size(202, 20);
            this.tbXmodem128.TabIndex = 3;
            // 
            // tbXmodem1k
            // 
            this.tbXmodem1k.Location = new System.Drawing.Point(198, 104);
            this.tbXmodem1k.Name = "tbXmodem1k";
            this.tbXmodem1k.Size = new System.Drawing.Size(202, 20);
            this.tbXmodem1k.TabIndex = 4;
            // 
            // tbYmodem
            // 
            this.tbYmodem.Location = new System.Drawing.Point(198, 144);
            this.tbYmodem.Name = "tbYmodem";
            this.tbYmodem.Size = new System.Drawing.Size(202, 20);
            this.tbYmodem.TabIndex = 5;
            // 
            // rtbStatus
            // 
            this.rtbStatus.Location = new System.Drawing.Point(202, 191);
            this.rtbStatus.Name = "rtbStatus";
            this.rtbStatus.Size = new System.Drawing.Size(543, 232);
            this.rtbStatus.TabIndex = 6;
            this.rtbStatus.Text = "";
            // 
            // btnWorkingDir
            // 
            this.btnWorkingDir.Location = new System.Drawing.Point(48, 2);
            this.btnWorkingDir.Name = "btnWorkingDir";
            this.btnWorkingDir.Size = new System.Drawing.Size(108, 23);
            this.btnWorkingDir.TabIndex = 7;
            this.btnWorkingDir.Text = "Working Directory";
            this.btnWorkingDir.UseVisualStyleBackColor = true;
            this.btnWorkingDir.Click += new System.EventHandler(this.BtnWorkingDir_Click);
            // 
            // tbWorkingDir
            // 
            this.tbWorkingDir.Location = new System.Drawing.Point(198, 5);
            this.tbWorkingDir.Name = "tbWorkingDir";
            this.tbWorkingDir.Size = new System.Drawing.Size(202, 20);
            this.tbWorkingDir.TabIndex = 8;
            this.tbWorkingDir.TextChanged += new System.EventHandler(this.tbWorkingDir_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(48, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "Receive";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(427, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 20);
            this.label2.TabIndex = 13;
            this.label2.Text = "Send";
            // 
            // btnYmodemSend
            // 
            this.btnYmodemSend.Location = new System.Drawing.Point(431, 141);
            this.btnYmodemSend.Name = "btnYmodemSend";
            this.btnYmodemSend.Size = new System.Drawing.Size(108, 23);
            this.btnYmodemSend.TabIndex = 12;
            this.btnYmodemSend.Text = "Ymodem";
            this.btnYmodemSend.UseVisualStyleBackColor = true;
            // 
            // btnModem1KSend
            // 
            this.btnModem1KSend.Location = new System.Drawing.Point(431, 101);
            this.btnModem1KSend.Name = "btnModem1KSend";
            this.btnModem1KSend.Size = new System.Drawing.Size(108, 23);
            this.btnModem1KSend.TabIndex = 11;
            this.btnModem1KSend.Text = "Xmodem 1k";
            this.btnModem1KSend.UseVisualStyleBackColor = true;
            // 
            // btnXmodem128Send
            // 
            this.btnXmodem128Send.Location = new System.Drawing.Point(431, 63);
            this.btnXmodem128Send.Name = "btnXmodem128Send";
            this.btnXmodem128Send.Size = new System.Drawing.Size(108, 23);
            this.btnXmodem128Send.TabIndex = 10;
            this.btnXmodem128Send.Text = "Xmodem 128 byte";
            this.btnXmodem128Send.UseVisualStyleBackColor = true;
            // 
            // Ymodem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnYmodemSend);
            this.Controls.Add(this.btnModem1KSend);
            this.Controls.Add(this.btnXmodem128Send);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbWorkingDir);
            this.Controls.Add(this.btnWorkingDir);
            this.Controls.Add(this.rtbStatus);
            this.Controls.Add(this.tbYmodem);
            this.Controls.Add(this.tbXmodem1k);
            this.Controls.Add(this.tbXmodem128);
            this.Controls.Add(this.btnYmodem);
            this.Controls.Add(this.btnModem1K);
            this.Controls.Add(this.btnXmodem128);
            this.Name = "Ymodem";
            this.Text = "File Transfer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Ymodem_FormClosed);
            this.Load += new System.EventHandler(this.Ymodem_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnXmodem128;
        private System.Windows.Forms.Button btnModem1K;
        private System.Windows.Forms.Button btnYmodem;
        private System.Windows.Forms.TextBox tbXmodem128;
        private System.Windows.Forms.TextBox tbXmodem1k;
        private System.Windows.Forms.TextBox tbYmodem;
        private System.Windows.Forms.Button btnWorkingDir;
        private System.Windows.Forms.TextBox tbWorkingDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        public System.Windows.Forms.RichTextBox rtbStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnYmodemSend;
        private System.Windows.Forms.Button btnModem1KSend;
        private System.Windows.Forms.Button btnXmodem128Send;
    }
}