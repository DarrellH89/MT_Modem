using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Diagnostics;
using System.Text;
using System.Drawing.Drawing2D;

namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        private SerialPort serialPort = new SerialPort();
        Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        int textCnt = 0;
        bool ymodem = false;
        private const int bufSize = 2048;
        private byte[] buf = new byte[bufSize];

        public MtMdm()
        {
            InitializeComponent();
            RefreshPortList();
            loadPreviousSessionSettings();
            setDisconnected();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialDataReceived);

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!ymodem && textCnt < term.Text.Length)
            {
                if (term.Text.Length < textCnt)
                    textCnt = term.TextLength;
                var txt = term.Text.Substring(textCnt);
                string msg = "Sent " + txt.Length + " bytes "+ txt;
                Debug.WriteLine(msg);
                textCnt = txt.Length + textCnt;
                try
                {
                    serialPort.Write(txt);
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Disconnected from serial port.", "Warning", MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                    //                setDisconnected();
                }
            }
            else
            {

            }

        }
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
        private void BtnConnect_Click(object sender, EventArgs e)
        {
               {
                serialPort.BaudRate = GetBaudRate();
                serialPort.PortName = GetPortName();
                serialPort.Parity = (Parity)0;// "None";
                serialPort.StopBits = (StopBits) 1;
                serialPort.DataBits = 8;
            }
            try
            {
                serialPort.Open();
                setConnected();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Could not connect to the COM port selected!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        //private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    System.Threading.Thread.Sleep(100);
        //    byte[] arr = new byte[serialPort.BytesToRead];
        //    serialPort.Read(arr, 0, arr.Length);
        //    //LogData("Port#1", arr);


        //}
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
                Invoke(new Action(() =>
                {
                    //read received message
                    string s = serialPort.ReadExisting();
                    string msg = "Rcvd " + s.Length +" byte " +s;
                    Debug.WriteLine(msg);
                    textCnt += s.Length;
                    term.Text += s;

                }));
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
        }

        private void MtMdm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveCurrentSessionSettings();
        }
        public void setConnected()
        {
            //scanButton.Enabled = false;
            BtnConnect.Enabled = false;
            BtnDrop.Enabled = true;
            term.Enabled = true;
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
            term.Enabled = false;
            statusBox.Text = "Not Connected.";
        }
        /************************************************************************
         * File Transfer
         */
        private void BtnYmodem_Click(object sender, EventArgs e)
        {

        }
        /**************************************************************************
         * Configuration files
         * */
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
                #endregion
            }
        }

        /// <summary>
        /// SAVES CURRENT SESSION SETTINGS SO THAT THEY CAN BE LOADED
        /// WHEN THE APP RUNS AGAIN.
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

            //set form items to default.
            BaudRate.SelectedItem = "19200";
           
            config.Save();
            #endregion

        }


    }
}
