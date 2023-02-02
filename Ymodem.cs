using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Management;
using System.Xml.Schema;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Collections.Specialized.BitVector32;


namespace MT_MDM
{
  public partial class Ymodem : Form
  {
      private int _errormax = 10;
      private int _transSize = 128;
      public static SerialBuffer serialData = new SerialBuffer();
      public static bool serialDataRdy = false;

      public Ymodem()
        {
            InitializeComponent();
        }
        private void Ymodem_Load(object sender, EventArgs e)
        {
            tbWorkingDir.Text = folderBrowserDialog1.SelectedPath;
            folderBrowserDialog1.ShowNewFolderButton = false;
            MtMdm.ymodem = true;
            tbWorkingDir.Text = MtMdm.downlLoadPath;

            //ticcnt = new Timer();
            //ticcnt.Interval = 100;           // 100 ms timer
            //ticcnt.Elapsed += new ElapsedEventHandler(TicksUpdate);
            //ticcnt.SynchronizingObject = this;
            //ticcnt.Start();
            //serialData = new SerialBuffer();        // publisher
            var serialIn = new SerialIn();              // subscriber
            serialData.SerialBufRdy += SerialIn.OnSerialBufRdy;
        }

        private void BtnWorkingDir_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                tbWorkingDir.Text = folderBrowserDialog1.SelectedPath;  
        }
        private void BtnXmodem128_Click(object sender, EventArgs e)
        {

            var fn = tbXmodem128.Text;
            MtMdm.ymodem = true;
            if (!ReceiveIt(fn))
                rtbStatus.Text += String.Format("File Receive Error " + fn );
        }

        private void BtnModem1K_Click(object sender, EventArgs e)
        {
            var fn = tbXmodem1k.Text;
            MtMdm.ymodem = true;
            if (!ReceiveIt(fn))
                rtbStatus.Text += String.Format("File Receive Error " + fn);
        }

        private void btnYmodem_Click(object sender, EventArgs e)
        {
            byte temp = 0;
            bool result = false;
            while(temp != 3)
            {
                result = SerialGetBuf(out temp, 10);
                rtbStatus.Text += String.Format("{0}", (char)temp);
                Debug.WriteLine("{0} {1}",result,(char)temp );

            }
        }

        private bool SerialGetBuf(out byte val, int time)
        {
            bool ok = false;
            DateTime timeEnd = DateTime.Now.AddSeconds(time);
            val = 0;                // dummy assignment

            while (DateTime.Now < timeEnd && !ok)
            {
                if (serialDataRdy)
                {
                    var test = SerialBuffer.GetData();
                    val = (byte)test;
                    if (test < 0x100)
                        ok = true;
                }
            }

            return ok;
        }


        //********************** ReadFile
        // Gets file names to receive. Should only be one name. Used
        private void ReadFile()
        {
            var filenames = Directory.GetFiles(tbWorkingDir.Text, "*.*");
            if (filenames == null)
            {
                MessageBox.Show("No Files Selcted", "File Error");
                return;
            }
            else
            {
                foreach (string fn in filenames)
                {
                    if (!ReceiveIt(fn))
                        MessageBox.Show("File Receive Error " + fn, "File Error");
                }
            }

        }
        //********************** ReceiveIt (fName)
        // Gets file fName from serial stream and writes it to disk
        private bool ReceiveIt(string fName )
        {
            var buf = new byte[32 * 1024];
            int bufPtr = 0;             // position to write next byte
            int bufPtrSave = 0;         // position at start of block transfer in case of failed transmission
            byte first, 
                scur,                   // current block count. Wraps to 0
                scomp;                  // current complimentary block count
            int snum = 0,               // Total number of sectors received
                errors = 0,
                errmax = _errormax,
                mode = 1, // initial mode for CRC, 0 for Checksum
                timeout = 5;           // initial timeout in seconds for CRC mode 
            byte chk1, chk2, chksum;
            UInt16 crc;
            FileStream fso ;
            BinaryWriter fileOutByte;

            bool ok = false;
            byte start = (byte) 'C';
            _transSize = 128;           // default packet size
 

            try
            {
            fso = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.Write);
            fileOutByte = new BinaryWriter(fso);  
            }
            catch (Exception e)
            {
                MessageBox.Show("File Receive Create Error " + fName, "File Error");
                return false;
            }

            rtbStatus.Text += "\nTrying to receive file\n";
              do  
              {
                  MtMdm.SendByte(start );               /* request CRC mode */
                  if(!SerialGetBuf(out first, timeout) ) /* look for SOH */
                      errors += 1 ;
              } 
              while ((first != A.SOH )&&(errors< 4 ));

              start = A.NAK ;        /* change to Checksum mode and for block NAK */
              if (errors > 3)
              {
                  timeout = 10 ;
                  rtbStatus.Text += "Changing to Checksum mode\n";
                  mode = 0 ;
                  MtMdm.SendByte(start);
                  do
                  {
                      if (!SerialGetBuf(out first, timeout)) /* look for SOH */
                          errors += 1;
                  } while ((first != A.SOH) && (errors < errmax));
              }

              if (errors > errmax)
                  return false;

              errors = 0;
              timeout = 5;
              do
              {
                ok = false;
                while (!((first == A.SOH) || (first == A.EOT) ))
                    if (!SerialGetBuf(out first, timeout)) /* look for SOH */
                    {
                        errors += 1;
                        rtbStatus.Text += String.Format("Error # {0} - No starting SOH, got {1}\n " , errors, first);
                        break;
                    } 
                if (first == A.SOH)
                {
                    bufPtrSave = bufPtr;
                    SerialGetBuf( out scur, timeout);          /*  real sector number  */
                    SerialGetBuf(out scomp, timeout);          /*  inverse of above */
                    rtbStatus.Text += String.Format("Header bytes {0} {1}\n", scur, scomp);
                    if ((scur + scomp) == 255)  /*<-- becomes this #*/
                    {
                        if (scur == (0xff & (snum + 1)))
                        {                    /* Good sector count */
                            switch (mode)
                            {
                                case 0:
                                    chksum = 0;
                                    for (var j = 0; j < _transSize; j++)
                                    {
                                        var temp = SerialGetBuf(out buf[bufPtr], timeout);
                                        chksum = (byte)((chksum + buf[bufPtr++]) & 0xff);
                                    }

                                    //byte getChkSum;
                                    SerialGetBuf(out var getChkSum, 1);
                                    if (getChkSum == chksum)   /* Checksum mode */
                                        ok = true;
                                    break;
                                case 1:
                                    crc = 0;             /* CRC mode */
                                    for (var j = 0; j < _transSize; j++)
                                    {
                                        if(!SerialGetBuf(out buf[bufPtr+j], timeout))
                                            rtbStatus.Text += String.Format("Serial Timeout Error\n");
                                        crc = (ushort)(crc ^ (ushort)buf[bufPtr+j] << 8);
                                        for (var i = 0; i < 8; i++)
                                            if ((ushort)(crc & 0x8000) >0)
                                                crc = (ushort)(crc << 1 ^ 0x1021);
                                            else
                                                crc = (ushort)(crc <<  1);
                                    }
                                    SerialGetBuf(out chk1, 1);           /* hi CRC byte */
                                    SerialGetBuf(out chk2, 1);     /* lo CRC byte */
                                    if (((chk1 << 8) + chk2) == crc)
                                        ok = true;
                                    break;
                            }
                            if (ok)
                            {
                                snum = snum + 1;
                                bufPtr += _transSize;
                                string sectorStr = String.Format("Received sector {0}", snum);
                                int p1 = rtbStatus.GetLineFromCharIndex(rtbStatus.MaxLength);
                                rtbStatus.Select(p1, rtbStatus.MaxLength);
                                rtbStatus.SelectedText = sectorStr;
                                errmax = errors + A.errormax;
                                /* Good block- Reset error maximum */

                                //            fileOutByte.Close();
                                //            fso.Dispose();
                                if (bufPtr >= buf.Length)
                                {                    /* check for full buffer */
                                    fileOutByte.Write(buf, 0, buf.Length);
                                    bufPtr = 0;
                                }
                            }
                            else
                                rtbStatus.Text += String.Format("\nError # {0} - Bad Block\n", errors + 1);
                        }
                        else
                            if (scur == (0xff & snum))
                            {                       /* wait until done */
                                while (SerialGetBuf(out var temp,2))  ;
                                rtbStatus.Text += String.Format("\nReceived duplicate sector {0}\n", scur);
                                start = A.ACK;   /* trick error handler */
                            }
                            else
                            {     /* fatal error */
                            rtbStatus.Text += String.Format("\nError # {0} - Synchronization error.\n", errors + 1);
                                start = A.EOT;
                                errors += A.errormax;
                            }
                    }
                    else
                        rtbStatus.Text += String.Format("\nError # %d - Sector number error. Got {0} Expected {1}\n", 
                                                        errors + 1, scur);
                }
                if (MtMdm.lastKey == A.CAN)      /* keyboard cancel using ^X */
                {
                    first = 0;
                    errors +=A.errormax;
                    ok = false;
                    start = A.EOT;    /* cancel on the other end */
                    MtMdm.SendByte(start);
                }
                if (ok)    /* got a good sector and finished all processing so - */
                    MtMdm.SendByte(A.ACK);           /* send request for next sector */
                else
                {                       /* some error occurred! */
                    errors++;
                    while (SerialGetBuf(out var temp, 2)) ;
                    MtMdm.SendByte(start);
                    start = A.NAK;
                }
                SerialGetBuf(out first, 1);
                 ;
              } while ((first != A.EOT) && (errors <= errmax));

              // File transfer complete - Cleanup
            if ((first == A.EOT) && (errors < errmax))
            {
                MtMdm.SendByte(A.ACK);
                rtbStatus.Text +="\nTransfer complete";
            }
            else
                rtbStatus.Text += "\nAborting\n";
            if (bufPtr >= buf.Length)       // Writing any data in buffer
            {                    
                fileOutByte.Write(buf, 0, buf.Length);
                bufPtr = 0;
            }
            return ok;
        }

        private void tbWorkingDir_TextChanged(object sender, EventArgs e)
        {
            MtMdm.SetDownLoadPath(tbWorkingDir.Text);
        }
    }
}
