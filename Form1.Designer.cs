namespace MT_MDM
{
    partial class MtMdm
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
            this.ComPort = new System.Windows.Forms.ComboBox();
            this.BaudRate = new System.Windows.Forms.ComboBox();
            this.term = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnConnect = new System.Windows.Forms.Button();
            this.BtnDrop = new System.Windows.Forms.Button();
            this.statusBox = new System.Windows.Forms.TextBox();
            this.BtnYmodem = new System.Windows.Forms.Button();
            this.btnTerm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ComPort
            // 
            this.ComPort.FormattingEnabled = true;
            this.ComPort.Location = new System.Drawing.Point(119, 36);
            this.ComPort.Name = "ComPort";
            this.ComPort.Size = new System.Drawing.Size(171, 21);
            this.ComPort.TabIndex = 0;
            this.ComPort.SelectedIndexChanged += new System.EventHandler(this.ComPort_SelectedIndexChanged);
            // 
            // BaudRate
            // 
            this.BaudRate.FormattingEnabled = true;
            this.BaudRate.Items.AddRange(new object[] {
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "56000",
            "57600",
            "115200",
            "128000",
            "256000"});
            this.BaudRate.Location = new System.Drawing.Point(476, 27);
            this.BaudRate.Name = "BaudRate";
            this.BaudRate.Size = new System.Drawing.Size(121, 21);
            this.BaudRate.TabIndex = 1;
            this.BaudRate.SelectedIndexChanged += new System.EventHandler(this.BaudRate_SelectedIndexChanged);
            // 
            // term
            // 
            this.term.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.term.Location = new System.Drawing.Point(57, 91);
            this.term.MinimumSize = new System.Drawing.Size(600, 100);
            this.term.Multiline = true;
            this.term.Name = "term";
            this.term.Size = new System.Drawing.Size(611, 386);
            this.term.TabIndex = 2;
            this.term.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(20, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "COM Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(386, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Baud Rate";
            // 
            // BtnConnect
            // 
            this.BtnConnect.Location = new System.Drawing.Point(713, 19);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Size = new System.Drawing.Size(81, 23);
            this.BtnConnect.TabIndex = 5;
            this.BtnConnect.Text = "Connect";
            this.BtnConnect.UseVisualStyleBackColor = true;
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // BtnDrop
            // 
            this.BtnDrop.Location = new System.Drawing.Point(713, 48);
            this.BtnDrop.Name = "BtnDrop";
            this.BtnDrop.Size = new System.Drawing.Size(81, 23);
            this.BtnDrop.TabIndex = 6;
            this.BtnDrop.Text = "Drop";
            this.BtnDrop.UseVisualStyleBackColor = true;
            this.BtnDrop.Click += new System.EventHandler(this.BtnDrop_Click);
            // 
            // statusBox
            // 
            this.statusBox.Location = new System.Drawing.Point(57, 508);
            this.statusBox.Name = "statusBox";
            this.statusBox.Size = new System.Drawing.Size(611, 20);
            this.statusBox.TabIndex = 7;
            // 
            // BtnYmodem
            // 
            this.BtnYmodem.Location = new System.Drawing.Point(713, 165);
            this.BtnYmodem.Name = "BtnYmodem";
            this.BtnYmodem.Size = new System.Drawing.Size(81, 23);
            this.BtnYmodem.TabIndex = 8;
            this.BtnYmodem.Text = "Ymodem";
            this.BtnYmodem.UseVisualStyleBackColor = true;
            this.BtnYmodem.Click += new System.EventHandler(this.BtnYmodem_Click);
            // 
            // btnTerm
            // 
            this.btnTerm.Location = new System.Drawing.Point(713, 91);
            this.btnTerm.Name = "btnTerm";
            this.btnTerm.Size = new System.Drawing.Size(81, 23);
            this.btnTerm.TabIndex = 9;
            this.btnTerm.Text = "Terminal";
            this.btnTerm.UseVisualStyleBackColor = true;
            this.btnTerm.Click += new System.EventHandler(this.btnTerm_Click);
            // 
            // MtMdm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 551);
            this.Controls.Add(this.btnTerm);
            this.Controls.Add(this.BtnYmodem);
            this.Controls.Add(this.statusBox);
            this.Controls.Add(this.BtnDrop);
            this.Controls.Add(this.BtnConnect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.term);
            this.Controls.Add(this.BaudRate);
            this.Controls.Add(this.ComPort);
            this.Name = "MtMdm";
            this.Text = "MT Modem";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MtMdm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ComPort;
        private System.Windows.Forms.ComboBox BaudRate;
        private System.Windows.Forms.TextBox term;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnConnect;
        private System.Windows.Forms.Button BtnDrop;
        private System.Windows.Forms.TextBox statusBox;
        private System.Windows.Forms.Button BtnYmodem;
        private System.Windows.Forms.Button btnTerm;
    }
}

