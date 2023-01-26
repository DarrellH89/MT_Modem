using System.Windows.Forms;

namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        public const byte
            CR = 0x0d,
            LF = 0x0a,
            BS = 0x08,
            ESC = 0x1b,
            DEL = 0x7f,
            ETX = 0x03,     // ^C
            NAK = 0x15,
            ACK = 0x06,
            SP = 0x20;


    }
}