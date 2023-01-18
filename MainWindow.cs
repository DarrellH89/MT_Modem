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
using System.Runtime.CompilerServices;
//using System.Windows.Forms.Timer;

namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        private SerialPort serialPort = new SerialPort();
        Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        int textCnt = 0;
        string textStr = "";
        bool ymodem = false;
        bool online = false;
        private const int bufSize = 1024 * 8;
        private char[] bufTerm = new char[80 * 25 + 100];
        private byte[] buf = new byte[bufSize];
        private string fontPath = "";
        //
        // not used
        [DllImport("KERNEL32.DLL", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        // end not used
        // Display variables
        private static int numCol = 80, 
                           numRow = 25, 
                           fontWidth = 8, 
                           fontHeight = 10;
        private static int charWidth = fontWidth* 2-2,          // # of pixels, 4 pixels per point
                           charHeight = (fontHeight * 2+8) +8;
                // 2 pixels per bit, 2 pixel spacer
        private int minW = numCol * charWidth +100 , 
                    minH = numRow * charHeight ;
        Color h19Color = Color.FromArgb(255, 0, 250, 0);
        Bitmap bm;
        GCHandle bmPixels;
        private UInt32[] bmPixMap;
        int cursorX = 0;        // Current cursor X position 80x25 grid
        int cursorY = 0;        // Current cursor Y position
        int cursorXlast = 0,    // last update cursor location
            cursorYlast = 0;
        bool ctlE = false;
        bool bmDirty = true;
        //
        // Timers
        private bool cursorVisible = true;
        private static Timer cursorTimer, h19Timer;
   
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
            statusBox.Enabled= false;

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
                setConnected();
                online = true;

            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Could not connect to the COM port selected!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        //
        //***************** Terminal mode - NOT CURRENTLY NEEDED - WILL DELETE
        private void btnTerm_Click(object sender, EventArgs e)
        {
            char dataOut;
            ConsoleKeyInfo dataIn;
            byte[] data = new byte[5];
            data[0] = 0;

            if (online)
            {
                do
                {
                    //AllocConsole();
                    //if (Console.KeyAvailable)
                    //{
                    //    dataIn = Console.ReadKey();
                    //    data[0] = (byte)dataIn.Key;
                    //    serialPort.Write(data, 0, 1);

                    //}
                    // check serial data
                    if (textStr != null)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(textStr);
                        foreach (byte ch in bytes)
                            displayChar(ch);
                        textStr = null;
                    }

                } while (data[0] != 5);

            }
            else
            {
                MessageBox.Show("Not Connected!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }
        //**************** End code to delete

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MtMdm_KeyDown(object sender, KeyEventArgs e)
        {
            byte[] ch = new byte[4];
            //if (e.Control && e.KeyCode == Keys.E)
            //    Close();
            ch[0] = (byte)e.KeyCode;
            if (!e.Shift)
                ch[0] += 0x20;
            if (online)                  // might need to check if Term key clicked
                displayChar(ch[0]);
            serialPort.Write(ch, 0,1);

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void statusBox_TextChanged(object sender, EventArgs e)
        {

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
        //************ Terminal Display Functions ************************


        private void display_init()
        {
            int x, y;

            //Rectangle rect = Screen.PrimaryScreen.Bounds;
            bmPixMap = new UInt32[minW * minH];
            bmPixels = GCHandle.Alloc(bmPixMap, GCHandleType.Pinned);
            bm = new Bitmap(minW, minH, minW * sizeof(Int32), PixelFormat.Format32bppPArgb, bmPixels.AddrOfPinnedObject());
            Color newColor = Color.FromArgb(0, 0, 0);
            for (x = 0; x < bm.Width; x++)
                for (y = 0; y < bm.Height; y++)
                    bm.SetPixel(x, y, newColor);
            termH19.Image = bm;
            cursorTimer = new Timer();   
            cursorTimer.Tick += new EventHandler(cursorUpdate);
            cursorTimer.Interval = 2000;    // 2 second interval
            cursorTimer.Start();
            h19Timer = new Timer();
            h19Timer.Tick += new EventHandler(displayUpdate);
            h19Timer.Interval = 16; // 60Hz = 16.67ms)
            h19Timer.Start();
        }
        private void displayChar(byte ch)
        {
            // Font 8 x 10
            //char ch;
            int charSize = charWidth*2;

            lock (bm)
            {
                if (ch > 0x1f && ch < 0x80)
                {
                    int cx = cursorX * charWidth;
                    int cy = cursorY * charHeight;
                    int x, y, ptrFont, t;
                    int mask = 128;


                    ptrFont = ch * 16;
                    
                    for (y = 0; y < 10; y++)
                    {
                        mask = 128;
                        for (x = 0; x < 8; x++)
                        {

                            t = h19.h19Font[ptrFont];
                            if ((h19.h19Font[ptrFont] & mask) > 1)
                            {
                                bm.SetPixel(cx + x*2, cy + y, h19Color);
                                bm.SetPixel(cx + x*2 + 1, cy + y, h19Color); 
                                bm.SetPixel(cx + x*2, cy + y+1, h19Color);
                                bm.SetPixel(cx + x*2 + 1, cy + y + 1, h19Color);
                            }
                            //cx += 2;
                            mask = mask / 2;
                        }
                        cy += 2;
                        ptrFont++;
                    }
                    cursorX ++;
                    if (cursorX == numCol )
                    {
                        cursorX = 0;
                        cursorY ++;
                    }

                    bmDirty = true;
                    //termH19.Image = bm; 
                    Invoke(new Action(() => { 
                        cursorBox.Text = cursorX.ToString() + "x" + cursorY.ToString(); 

                        }));              

                }
            }
        }

        private void cursorUpdate(Object myObject,
                                            EventArgs myEventArgs)
        {
            Color offColor = Color.FromArgb(0, 0, 0);
            lock (bm)
            {
                cursorVisible = !cursorVisible;
                drawCursor(cursorVisible ? h19Color : offColor);
                bmDirty = true;
            }
        }
        private void drawCursor (Color what)
        {
            int cx = cursorX * charWidth ,
                cy = cursorY * charHeight + charHeight,
                cxlast = cursorXlast * charWidth,
                cylast = cursorYlast * charHeight + charHeight;
            Color offColor = Color.FromArgb(0, 0, 0);
            lock (bm)
            {
                if ((cursorX != cursorXlast) || (cursorY != cursorYlast))        // cursor moved
                {
                    for (int j = 0; j < 16; j++)
                        bm.SetPixel(cxlast + j, cylast, offColor);
                    cursorYlast = cursorY;
                    cursorXlast = cursorX;
                }
                for (int j = 0; j < 16; j++)
                     bm.SetPixel(cx + j, cy, what);
            }
        }
        private void displayUpdate(Object myObject,
                                            EventArgs myEventArgs)
        {
            if (bmDirty)
            {
                lock (termH19)
                {
                    termH19.Refresh();
                    bmDirty = false;
                }
            }
        }
       //
       //****************** Serial Port Functions *******************
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
            // Overall Processing
            /*invoke because this event is not in main thead. only main thread can change the
             *form items. invoke makes a call to the form thread and form thread does what invoke
             *says. in our case it will write to the history box.
             * 
             */
            if (!ymodem)
            {
                //Invoke(new Action(() =>
                //{
                //    //read received message
                //    string s = serialPort.ReadExisting();
                //    string msg = "Rcvd " + s.Length + " byte " + s;
                //    Debug.WriteLine(msg);
                //    textCnt += s.Length;  // WHAT is this for?
                //    textStr += s;
                //}));
                int data= serialPort.ReadByte();
                while(data != -1)
                {
                    displayChar((byte)data);
                    data = serialPort.ReadByte();
                }
      
            }
            else
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
            termH19.Enabled = true;
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
            termH19.Enabled = false;
            statusBox.Text = "Not Connected.";
        }
        //
        //*********************** File Transfer
        //
        private void BtnYmodem_Click(object sender, EventArgs e)
        {

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

        //
        //******************* Font Class h19 ************************************
        public partial class h19
        {
            public static byte[] h19Font = new byte[4096];
            public static bool fontOK = false;
        }

    }
}
