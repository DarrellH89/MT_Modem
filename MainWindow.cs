using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Imaging;


namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        private SerialPort serialPort = new SerialPort();
        Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

        string textStr = "";
        bool ymodem = false;
        bool online = false;
        bool mouseCapture = false;
        private static int maxCol = 81, maxRow = 100;
        private Byte[] display = new Byte[maxRow*(maxCol)];

        private string fontPath = "";
        //[DllImport("KERNEL32.DLL", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();
        // Display variables
        private static int numCol = 80, numRow = 25, charWidth = 8, charHeight = 10;
                // 2 pixels per bit, 2 pixel spacer
         int cursorX = 0;        // Current cursor X position 80x25 grid
        int cursorY = 75;        // Current cursor Y position
        bool ctlE = false;
        private float defaultRtbH, defaultRtbW;
        //

        //
        //***************** Form Controls ****************************//
        public MtMdm()
        {
            InitializeComponent();
            RefreshPortList();
            loadPreviousSessionSettings();
            setDisconnected();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialDataReceived);
            display_init();
            cursorBox.Text = cursorX.ToString() + "x" + (cursorY - 75).ToString();
            statusBox.Enabled= false;

            //richTextBox1.Select(1, 1);
            h19Term.Focus();
            defaultRtbH = h19Term.Size.Height;
            defaultRtbW = h19Term.Size.Width;
           }
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            {
                serialPort.BaudRate = GetBaudRate();
                serialPort.PortName = GetPortName();
                serialPort.Parity = (Parity)0;// "None";
                serialPort.StopBits = (StopBits)1;
                serialPort.DataBits = 8;
            }
            try
            {
                serialPort.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Could not connect to the COM port selected!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            setConnected();
            online = true;
        }       
     

        private void btnFont_Click(object sender, EventArgs e)
        {
            loadFont("");
        }
        private void MtMdm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveCurrentSessionSettings();
        }
        //
    
        //
        //****************** Serial Port Functions *******************
        //
        private void RefreshPortList()
        {
            ComPort.Items.Clear();
            foreach (string port in GetPortNames())
            {
                ComPort.Items.Add(port);
            }
            if (ComPort.Items.Count > 0)
                ComPort.SelectedIndex = 0;
        }
        private List<string> GetPortNames()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();
                foreach (string port in portnames)
                {
                    bool founded = false;
                    foreach (string iport in tList)
                    {
                        if (iport.Contains(port))
                        {
                            founded = true;
                        }
                    }
                    if (!founded)
                    {
                        tList.Add(port);
                        break;
                    }
                }
                return tList;
            }
        }
        private int GetBaudRate()
        {
            string temp = BaudRate.Items[BaudRate.SelectedIndex] as string;
            int ret = int.Parse(temp);
            return ret;
        }
        private string GetPortName()
        {
            string temp = ComPort.Items[ComPort.SelectedIndex] as string;
            temp += ' ';
            return temp.Substring(0, temp.IndexOf(' '));
        }
 

        private void serialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!ymodem)
            {
                int data= serialPort.ReadByte();
                while(data != -1)
                {
                    Invoke(new Action(() => {
                    displayChar((byte)data);
                    }));

                    try
                    {
                        data = serialPort.ReadByte();
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
            else                // Ymodem file transfer code
            {

            }
        }
        private void ComPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void BaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void BtnDrop_Click(object sender, EventArgs e)
        {
            setDisconnected();
            online = false;
        }


        public void setConnected()
        {
            //scanButton.Enabled = false;
            BtnConnect.Enabled = false;
            BtnDrop.Enabled = true;
            statusBox.Text = "Connected.";
        }

        /// <summary>
        /// RESET UI SO THAT USER CAN CONNECT TO ANOTHER SERIAL PORT
        /// IF NECESSARY
        /// </summary>
        public void setDisconnected()
        {
            serialPort.Close();
            //scanButton.Enabled = false;
            BtnConnect.Enabled = true;
            BtnDrop.Enabled = false;
            statusBox.Text = "Not Connected.";
        }
        //
        //*********************** File Transfer
        //
        private void BtnYmodem_Click(object sender, EventArgs e)
        {
    

        }
        //************ Terminal Display Functions ************************
        // main form key press method, not currently used
        private void MtMdm_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte[] ch = new byte[1];
            ch[0] = (byte)e.KeyChar;
            Debug.WriteLine("MtMdm: char {0}, value {1}", (char)ch[0], BitConverter.ToString(ch));
            displayChar(ch[0]);
            e.Handled = true;       // Tell Windows no further action is needed to keep RTB from updating screen

        }

        private void h19Term_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte[] ch = new byte[1];
            //if (e.Control && e.KeyCode == Keys.E)
            //    Close();
            ch[0] = (byte)e.KeyChar;
            if (ch[0] > 0x19 && ch[0] < 0x7f)  
                Debug.WriteLine("h19Term: char {0}, value {1}", (char)ch[0],  BitConverter.ToString(ch));
            else
                Debug.WriteLine("h19Term: value {0}", ch[0]);
            displayChar(ch[0]);

            e.Handled = true;
            //if (online)                  // might need to check if Term key clicked
            //    displayChar(ch[0]);
            try
            {
                serialPort.Write(ch, 0, 1);
            }
            catch(Exception ex)
            {

            }
        }

        private void h19Term_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (mouseCapture)
            {
                textBox1.Text = "";
                mouseCapture = false;
            }
            else
            {
                textBox1.Text = "Mouse Capture";
                mouseCapture = true;
            }
            var s1 = h19Term.SelectedText;
            if(s1.Length > 0)
                Clipboard.SetText(h19Term.SelectedText);
        }

        private void h19Term_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void h19Term_Resize(object sender, EventArgs e)
        {
            float newH, newW;
            newH = h19Term.Size.Height;
            newW = h19Term.Size.Width;
            if (defaultRtbH * defaultRtbW > 0)
                h19Term.ZoomFactor = (newH * newW) / (defaultRtbW * defaultRtbH);
        }


        private void btnClrScreen_Click(object sender, EventArgs e)
        {
            display_init();
        }



        //
        //*************************** Configuration files
        //
        public void initSettings()
        {
            //clear so if program messed up and already there is a
            // config file adding doesnt duplicate values
            config.AppSettings.Settings.Clear();

            /*
             * ADD THE DEFAULT VALUES TO THE CONFIG FILE
             */
            #region Init first time run config variables
            config.AppSettings.Settings.Add("baud", "6");
            config.AppSettings.Settings.Add("bitRate", "3");
            config.AppSettings.Settings.Add("parity", "0");
            config.AppSettings.Settings.Add("stopBits", "0");
            config.AppSettings.Settings.Add("CR", "true");

            config.AppSettings.Settings.Add("form_x", "0");
            config.AppSettings.Settings.Add("form_y", "0");

            config.AppSettings.Settings.Add("form_width", "580");
            config.AppSettings.Settings.Add("form_height", "640");
            config.AppSettings.Settings.Add("fontPath","c:\\");
            #endregion

            //set form items to default.
            BaudRate.SelectedItem = "19200";
            //bitRateComboBox.SelectedItem = "8 Bits";
            //parityComboBox.SelectedItem = "None";
            //stopBitsComboBox.SelectedIndex = 0;
            //checkBoxCR.Checked = true;

            //save the config file changes
            config.Save();
        }


        /// <summary>
        /// USE TO LOAD PREVIOUS SESSION SETTINGS FROM CONFIG FILE
        /// (BAUDRATE,BITRATE,FORMX,Y,WIDTH...)
        /// </summary>
        public void loadPreviousSessionSettings()
        {
            //SETUP ALL THE VARIABLES WHICH WILL BE USED
            #region init variables
            bool reint = false;
            int baud = 6;
            int bitRate = 0;
            int parity = 0;
            int stopBits = 0;

            int form_x = 0;
            int form_y = 0;
            int form_width = 0;
            int form_height = 0;

            bool cr = true;
            string font_path = null;
            #endregion

            //try catch to make sure user didnt mess the xml file and added characters
            //and other non int (in cr case non boolean value) values into it.
            try
            {
                baud = int.Parse(config.AppSettings.Settings["baud"].Value);
                bitRate = int.Parse(config.AppSettings.Settings["bitRate"].Value);
                parity = int.Parse(config.AppSettings.Settings["parity"].Value);
                stopBits = int.Parse(config.AppSettings.Settings["stopBits"].Value);
                form_x = int.Parse(config.AppSettings.Settings["form_x"].Value);
                form_y = int.Parse(config.AppSettings.Settings["form_y"].Value);
                form_width = int.Parse(config.AppSettings.Settings["form_width"].Value);
                form_height = int.Parse(config.AppSettings.Settings["form_height"].Value);
                cr = Boolean.Parse(config.AppSettings.Settings["CR"].Value);
                font_path = (config.AppSettings.Settings["fontPath"].Value);
            }
            catch (Exception)
            {
                reint = true;   //if exception caught. reint= true;
            }

            //ifs to check; int values are in range of combobox indexes.
            //protects from naughty users who are trying to break the code
            //by changing the xml values.
            #region range check for int values
            if (baud > 13 || baud < 0) reint = true;
            if (bitRate > 3 || bitRate < 0) reint = true;
            if (parity > 4 || parity < 0) reint = true;
            if (stopBits > 3 || stopBits < 0) reint = true;
            if (form_width < 400) reint = true;
            if (form_height < 400) reint = true;
            #endregion

            //if something is messed up (reint == true)
            if (reint)
            {
                //show error/info message and call initSettings() which will reset
                //the values to default
                MessageBox.Show("Config file Error!\nFirst time running the application or\ncorrupt config file.\nFixed!", "");
                initSettings();
            }
            else
            {
                //if everything is right set the values taken from
                //xml file to the comboBoxes, form x,y,width etc..
                #region Setup form according to xml file values
                BaudRate.SelectedIndex = baud;
                //BaudRate.SelectedIndex = baud;
                //bitRateComboBox.SelectedIndex = bitRate;
                //parityComboBox.SelectedIndex = parity;
                //stopBitsComboBox.SelectedIndex = stopBits;
                //checkBoxCR.Checked = cr;
                this.Left = form_x;
                this.Top = form_y;
                this.Height = form_height;
                this.Width = form_width;
                this.fontPath= font_path;
                loadFont(font_path);
                #endregion
            }
        }

        /// <summary>
        /// SAVES CURRENT SESSION SETTINGS SO THAT THEY CAN BE LOADED
        /// WHEN THE APP RUNS AGAIN.
        /// Learned from https://github.com/hushoca/c-serial-terminal
        /// </summary>
        public void saveCurrentSessionSettings()
        {

            config.AppSettings.Settings.Clear();

            #region set variables
            String baudRate = BaudRate.SelectedIndex.ToString();
            String bitRate = "3"; // bitRateComboBox.SelectedIndex.ToString();
            String parity = "0"; // parityComboBox.SelectedIndex.ToString();
            String stopBits = "1"; // stopBitsComboBox.SelectedIndex.ToString();

            String CR = "True "; // checkBoxCR.Checked.ToString();
            String form_height = this.Height.ToString();
            String form_width = this.Width.ToString();
            String form_x = this.Location.X.ToString();
            String form_y = this.Location.Y.ToString();
            #endregion

            #region set variables in the config file
            config.AppSettings.Settings.Add("baud", baudRate);
            config.AppSettings.Settings.Add("bitRate", bitRate);
            config.AppSettings.Settings.Add("parity", parity);
            config.AppSettings.Settings.Add("stopBits", stopBits);
            config.AppSettings.Settings.Add("CR", CR);

            config.AppSettings.Settings.Add("form_x", form_x);
            config.AppSettings.Settings.Add("form_y", form_y);

            config.AppSettings.Settings.Add("form_width", form_width);
            config.AppSettings.Settings.Add("form_height", form_height);
            config.AppSettings.Settings.Add("fontPath", fontPath);
            //set form items to default.
            BaudRate.SelectedItem = "19200";
           
            config.Save();
            #endregion

        }

        // No longer needed while using TrueType Font
        //       
        //******************* Load Font ******************
        void loadFont(string path)
        {
            if (path.Length == 0)
            {
                OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                openFileDialog1.Filter = "Font Files (*.bin)|*.bin";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.CheckFileExists = false;
                openFileDialog1.ShowDialog();
                path = openFileDialog1.FileName;
                if (path.Length == 0)
                {
                    statusBox.Text = ("No font files found");
                    return;
                }
                else
                {
                    fontPath = path;
                    saveCurrentSessionSettings();
                }
            }
            fontBox.Text = fontPath.Substring(fontPath.LastIndexOf("\\") + 1);   
            h19.h19Font = File.ReadAllBytes(path);
            h19.fontOK = true;
        }


        //******************* Font Class h19 ************************************
        public partial class h19
        {
            public static byte[] h19Font = new byte[4096];
            public static bool fontOK = false;
        }

    }
}
