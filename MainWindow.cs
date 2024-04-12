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
using System.Security.Policy;


namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

        string textStr = "";
        public static bool ymodem = false;
        public static bool online = false;
        public static byte lastKey = 0;
        public static bool serialBufFull = false;
        bool mouseCapture = false;
        private static int maxCol = 81, maxRow = 100;
        private Byte[] display = new Byte[maxRow*(maxCol)];

        private string fontPath = "";
        private string appPath = "c:\\";
        public static string downlLoadPath = "c:\\";
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
        // Serial in buffer for file transfer
        private static byte[] _dataIn = new byte[1024];
        private static int _dataInPtr = 0;
        private static int _dataInEnd = 0;
        private static bool _dataFull = false;

        private static bool _echo = false;
        // Locking variables
        //public static object syncObj = new object();
        //private static Queue <byte> serialBuffer = new Queue< byte>(); 
        // serial port values
        //public static int gBaudRate = 0;
        //public static string gPortName ="COM2";
        //public static SerialPort serialPort = new SerialPort(); 
        public static SerialBuffer serialPort = new SerialBuffer();




        //

        //
        //***************** Form Controls ****************************//
        public MtMdm()
        {
            //Program.Name = "VT52";
            //this.Text = Program.Name; 
            InitializeComponent();

           }
        private void MtMdm_Load(object sender, EventArgs e)
        {
            RefreshPortList();
            loadPreviousSessionSettings();
            SetDisconnected();
            //serialPort.DataReceived += new SerialDataReceivedEventHandler(serialDataReceived);
            Display_init();
            cursorBox.Text = cursorX.ToString() + "x" + (cursorY - 75).ToString();
            statusBox.Enabled= false;
            fontComboBox.Text = h19Term.Font.Name;
            LoadFont();
            //richTextBox1.Select(1, 1);
            h19Term.Focus();
            defaultRtbH = h19Term.Size.Height;
            defaultRtbW = h19Term.Size.Width;
        }
        //protected override void OnHandleCreated(EventArgs e)
        //{
        //    base.OnHandleCreated(e);
        //    IntPtr hMenu = Win32.GetSystemMenu(this.Handle, false);
        //    Win32.AppendMenu(hMenu, MF.SEPARATOR, (UIntPtr)0);
        //    Win32.AppendMenu(hMenu, MF.STRING, (UIntPtr)5, "Settings\tF5");
        //    Win32.AppendMenu(hMenu, MF.STRING, (UIntPtr)6, "Connection\tF6");
        //    Win32.AppendMenu(hMenu, MF.STRING, (UIntPtr)11, "Brightness -\tF11");
        //    Win32.AppendMenu(hMenu, MF.STRING, (UIntPtr)12, "Brightness +\tF12");
        //    Win32.AppendMenu(hMenu, MF.SEPARATOR, (UIntPtr)0);
        //    Win32.AppendMenu(hMenu, MF.STRING, (UIntPtr)99, String.Concat("About ", Program.Name));
        //}
        public static void SetDownLoadPath(string path)
        {
            downlLoadPath = path;
            //saveCurrentSessionSettings();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            ConnectPort();

        }

        private void ConnectPort()
        {
            //serialPort.BaudRate = GetBaudRate();
            //serialPort.PortName = GetPortName();
            //serialPort.Parity = (Parity)0;// "None";
            //serialPort.StopBits = (StopBits)1;
            //serialPort.DataBits = 8;
            //try
            //{
            //    serialPort.Open();
            //}
            //catch (Exception)
            //{
            //    ExMessage("Could not connect to the COM port selected!");
            //}
            if (serialPort.Connect(GetBaudRate(), GetPortName()))
            {
                serialPort.SerialData += OnSerialData;
                SetConnected();
                online = true;
            }

        }

        private void BtnFont_Click(object sender, EventArgs e)
        {
            LoadFont();
        }
        private void BtnEcho_Click(object sender, EventArgs e)
        {
            _echo = !_echo;
            if (_echo)
                btnEcho.Text = "Echo On";
            else
                btnEcho.Text = "Echo Off";
        }
        private void BtnYmodem_Click(object sender, EventArgs e)
        {
            var fileTransfer = new Ymodem();
            fileTransfer.ShowDialog();
            fileTransfer.Close();


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
                var portNames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portNames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();
                foreach (var port in portNames)
                {
                    bool found = false;
                    foreach (string iport in tList)
                    {
                        if (iport.Contains(port))
                        {
                            found = true;
                        }
                    }
                    if (!found)
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
            int data = 0;

            try
            {
               // data = serialPort.ReadByte();
            }
            catch (Exception ex)
            {
                ExMessage("Serial Read Error!");
            }
            if (!ymodem)
            {
                Invoke(new Action(() => {
                    DisplayChar((byte)data);
                }));
            }
            else                // Ymodem file transfer code
            {
                //if (!SerialInBuf((byte)data))    // false if serial data buffer is full
                //    serialBufFull = true;               // serial buffer full flag

                //Ymodem.serialData.AddData((byte) data);
            }
            //if (!ymodem)
            //{
            //    data= serialPort.ReadByte();
            //    while(data != -1)
            //    {
            //        Invoke(new Action(() => {
            //        DisplayChar((byte)data);
            //        }));
            //        try
            //        {
            //            data = serialPort.ReadByte();
            //        }
            //        catch(Exception ex)
            //        {
            //            ExMessage("Serial Read Error!");
            //        }
            //    }
            //}
            //else                // Ymodem file transfer code
            //{
            //    if (!Ymodem.SerialInBuf((byte)data))
            //        serialBufFull = true;               // serial buffer full flag
            //}
        }
        private bool SerialInBuf(byte num)
        {
            int length = _dataIn.Length;
            bool result = true;

            if (!_dataFull)
            {
                _dataIn[_dataInEnd++] = num;
                _dataInEnd %= length;
            }
            if (_dataInEnd == _dataInPtr)
            {
                _dataFull = true;
                result = false;

            }

            return result;
        }

        // ************* SerialGetBuf - gets byte from Serial In buffer
        // ok = valid result
        public static bool SerialGetBuf(out byte val, int time)
        {
            int length = _dataIn.Length;
            DateTime timeEnd = DateTime.Now.AddSeconds(time);
            bool ok = true;
            val = 0;

            while (ok )
            {
                //if (_dataInPtr != _dataInEnd)
                //{
                //    val = _dataIn[_dataInPtr++];
                //    _dataInPtr %= length;
                //    break;
                //}
                //else
                //{           // Check elapsed time for timeout
                //    if (DateTime.Now > timeEnd)
                //        ok = false;
                //}
                if (DateTime.Now > timeEnd)
                    ok = false;
                //lock (syncObj)
                //{
                //    if (serialBuffer.Count > 0)
                //    {
                //        val = serialBuffer.Dequeue();
                //        break;
                //    }
                //}
            }
            return ok;
        }
        private void BtnDrop_Click(object sender, EventArgs e)
        {
            SetDisconnected();
            online = false;
        }


        public void SetConnected()
        {
            //scanButton.Enabled = false;
            BtnConnect.Enabled = false;
            BtnDrop.Enabled = true;
            statusBox.Text = "Connected.";
        }

 
        public void SetDisconnected()
        {
            //serialPort.Close();    NEED TO ADD SOME CODE
            //scanButton.Enabled = false;
            BtnConnect.Enabled = true;
            BtnDrop.Enabled = false;
            statusBox.Text = "Not Connected.";
        }
        //
        //*********************** File Transfer
        //

        //************ Terminal Display Functions ************************
        // main form key press method, not currently used
        private void MtMdm_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte[] ch = new byte[1];
            ch[0] = (byte)e.KeyChar;
            Debug.WriteLine("MtMdm: char {0}, value {1}", (char)ch[0], BitConverter.ToString(ch));
            if (_echo)
                DisplayChar(ch[0]);
            e.Handled = true;       // Tell Windows no further action is needed to keep RTB from updating screen

        }

        private void H19Term_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte ch ;
            //if (e.Control && e.KeyCode == Keys.E)
            //    Close();
            ch = (byte)e.KeyChar;
            if (ch > 0x19 && ch < 0x7f)  
                Debug.WriteLine("h19Term: char {0}, value {1}", (char)ch,  ch);
            else
                Debug.WriteLine("h19Term: value {0}", ch);
            DisplayChar(ch);

            e.Handled = true;
            if (!ymodem)                  // don't interrupt file transfer
                SendByte(ch);
            ;

        }

        private void SendByte(byte val)
        {
            //byte[] ch = new byte[1];
            //ch[0] = val;
            if (online)
                try
                {
                    serialPort.Send(val);    
                    Debug.WriteLine("Sent {0:X}", val);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Serial Write Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void H19Term_MouseCaptureChanged(object sender, EventArgs e)
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

        private void H19Term_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void H19Term_Resize(object sender, EventArgs e)
        {
            float newH, newW;
            newH = h19Term.Size.Height;
            newW = h19Term.Size.Width;
            if (defaultRtbH * defaultRtbW > 0)
                h19Term.ZoomFactor = (newH * newW) / (defaultRtbW * defaultRtbH);
        }


        private void BtnClrScreen_Click(object sender, EventArgs e)
        {
            Display_init();
        }


        public void ExMessage(String s)
        {
            MessageBox.Show(s, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        //
        //*************************** Configuration files
        /// Learned from https://github.com/hushoca/c-serial-terminal
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
            config.AppSettings.Settings.Add("appPath", Directory.GetCurrentDirectory()) ;
            config.AppSettings.Settings.Add("appPath", downlLoadPath);

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
            string app_path = "C:\\";
            string dl_path = "C:\\";
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
                font_path = (config.AppSettings.Settings["fontPath"].Value); 
                app_path = (config.AppSettings.Settings["appPath"].Value);
                dl_path = (config.AppSettings.Settings["dlPath"].Value);
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
                //bitRateComboBox.SelectedIndex = bitRate;
                //parityComboBox.SelectedIndex = parity;
                //stopBitsComboBox.SelectedIndex = stopBits;
                this.Left = form_x;
                this.Top = form_y;
                this.Height = form_height;
                this.Width = form_width;
                this.fontPath= font_path;
                this.appPath= app_path;
                downlLoadPath = dl_path;
                LoadFont();
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

            config.AppSettings.Settings.Add("form_x", form_x);
            config.AppSettings.Settings.Add("form_y", form_y);

            config.AppSettings.Settings.Add("form_width", form_width);
            config.AppSettings.Settings.Add("form_height", form_height);
            config.AppSettings.Settings.Add("fontPath", fontPath);
            config.AppSettings.Settings.Add("appPath", appPath);
            config.AppSettings.Settings.Add("dlPath", downlLoadPath);
            BaudRate.SelectedItem = "19200";
           
            config.Save();
            #endregion

        }
        // End Configuration Code
        //
  

       

        //       
        //******************* Load Font ******************
        private void LoadFont()
        {
            int h19item = 0, cnt = 0;

            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                fontComboBox.Items.Add(font.Name);
                if (font.Name.Contains("Heathkit"))
                    h19item = cnt;
                cnt++;
            }
            fontComboBox.SelectedIndex = h19item;

        }
        private void FontComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var s = fontComboBox.SelectedItem.ToString();
            h19Term.Font = new Font(s, 12, FontStyle.Regular);
            fontPath = s.ToString();
            saveCurrentSessionSettings();
        }

    }
}
