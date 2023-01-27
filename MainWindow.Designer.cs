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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnConnect = new System.Windows.Forms.Button();
            this.BtnDrop = new System.Windows.Forms.Button();
            this.statusBox = new System.Windows.Forms.TextBox();
            this.BtnYmodem = new System.Windows.Forms.Button();
            this.btnClrScreen = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cursorBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.fontBox = new System.Windows.Forms.TextBox();
            this.btnFont = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.h19Term = new System.Windows.Forms.RichTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ComPort
            // 
            this.ComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComPort.FormattingEnabled = true;
            this.ComPort.Location = new System.Drawing.Point(86, 5);
            this.ComPort.Name = "ComPort";
            this.ComPort.Size = new System.Drawing.Size(114, 21);
            this.ComPort.TabIndex = 0;
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
            this.BaudRate.Location = new System.Drawing.Point(86, 31);
            this.BaudRate.Name = "BaudRate";
            this.BaudRate.Size = new System.Drawing.Size(114, 21);
            this.BaudRate.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "COM Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Baud Rate";
            // 
            // BtnConnect
            // 
            this.BtnConnect.Location = new System.Drawing.Point(361, 0);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Size = new System.Drawing.Size(81, 23);
            this.BtnConnect.TabIndex = 5;
            this.BtnConnect.Text = "Connect";
            this.BtnConnect.UseVisualStyleBackColor = true;
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // BtnDrop
            // 
            this.BtnDrop.Location = new System.Drawing.Point(361, 32);
            this.BtnDrop.Name = "BtnDrop";
            this.BtnDrop.Size = new System.Drawing.Size(81, 23);
            this.BtnDrop.TabIndex = 6;
            this.BtnDrop.Text = "Drop";
            this.BtnDrop.UseVisualStyleBackColor = true;
            this.BtnDrop.Click += new System.EventHandler(this.BtnDrop_Click);
            // 
            // statusBox
            // 
            this.statusBox.Location = new System.Drawing.Point(627, 34);
            this.statusBox.Name = "statusBox";
            this.statusBox.Size = new System.Drawing.Size(117, 20);
            this.statusBox.TabIndex = 7;
            // 
            // BtnYmodem
            // 
            this.BtnYmodem.Location = new System.Drawing.Point(512, 32);
            this.BtnYmodem.Name = "BtnYmodem";
            this.BtnYmodem.Size = new System.Drawing.Size(81, 23);
            this.BtnYmodem.TabIndex = 8;
            this.BtnYmodem.Text = "Ymodem";
            this.BtnYmodem.UseVisualStyleBackColor = true;
            this.BtnYmodem.Click += new System.EventHandler(this.BtnYmodem_Click);
            // 
            // btnClrScreen
            // 
            this.btnClrScreen.Location = new System.Drawing.Point(512, 0);
            this.btnClrScreen.Name = "btnClrScreen";
            this.btnClrScreen.Size = new System.Drawing.Size(81, 23);
            this.btnClrScreen.TabIndex = 9;
            this.btnClrScreen.Text = "Clear Screen";
            this.btnClrScreen.UseVisualStyleBackColor = true;
            this.btnClrScreen.Click += new System.EventHandler(this.btnClrScreen_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.cursorBox);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.fontBox);
            this.panel1.Controls.Add(this.btnFont);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnClrScreen);
            this.panel1.Controls.Add(this.statusBox);
            this.panel1.Controls.Add(this.BtnYmodem);
            this.panel1.Controls.Add(this.ComPort);
            this.panel1.Controls.Add(this.BaudRate);
            this.panel1.Controls.Add(this.BtnDrop);
            this.panel1.Controls.Add(this.BtnConnect);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 651);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(895, 58);
            this.panel1.TabIndex = 10;
            // 
            // cursorBox
            // 
            this.cursorBox.Location = new System.Drawing.Point(218, 35);
            this.cursorBox.Name = "cursorBox";
            this.cursorBox.Size = new System.Drawing.Size(92, 20);
            this.cursorBox.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(221, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Cursor X x Y";
            // 
            // fontBox
            // 
            this.fontBox.Location = new System.Drawing.Point(778, 34);
            this.fontBox.Name = "fontBox";
            this.fontBox.Size = new System.Drawing.Size(96, 20);
            this.fontBox.TabIndex = 13;
            // 
            // btnFont
            // 
            this.btnFont.Location = new System.Drawing.Point(778, 3);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(81, 23);
            this.btnFont.TabIndex = 12;
            this.btnFont.Text = "Font";
            this.btnFont.UseVisualStyleBackColor = true;
            this.btnFont.Click += new System.EventHandler(this.btnFont_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(624, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Status";
            // 
            // h19Term
            // 
            this.h19Term.BackColor = System.Drawing.SystemColors.InfoText;
            this.h19Term.Dock = System.Windows.Forms.DockStyle.Top;
            this.h19Term.EnableAutoDragDrop = true;
            this.h19Term.Font = new System.Drawing.Font("Heathkit19", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.h19Term.ForeColor = System.Drawing.Color.LawnGreen;
            this.h19Term.Location = new System.Drawing.Point(0, 0);
            this.h19Term.Name = "h19Term";
            this.h19Term.RightMargin = 910;
            this.h19Term.Size = new System.Drawing.Size(1121, 630);
            this.h19Term.TabIndex = 12;
            this.h19Term.Text = "";
            this.h19Term.WordWrap = false;
            this.h19Term.KeyDown += new System.Windows.Forms.KeyEventHandler(this.h19Term_KeyDown);
            this.h19Term.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.h19Term_KeyPress);
            this.h19Term.MouseCaptureChanged += new System.EventHandler(this.h19Term_MouseCaptureChanged);
            this.h19Term.Resize += new System.EventHandler(this.h19Term_Resize);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(941, 632);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(151, 20);
            this.textBox1.TabIndex = 13;
            // 
            // MtMdm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1121, 721);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.h19Term);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.Name = "MtMdm";
            this.Text = "MT Modem";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MtMdm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ComPort;
        private System.Windows.Forms.ComboBox BaudRate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnConnect;
        private System.Windows.Forms.Button BtnDrop;
        private System.Windows.Forms.TextBox statusBox;
        private System.Windows.Forms.Button BtnYmodem;
        private System.Windows.Forms.Button btnClrScreen;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnFont;
        private System.Windows.Forms.TextBox fontBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox cursorBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TextBox textBox1;
        protected internal System.Windows.Forms.RichTextBox h19Term;
    }
}

