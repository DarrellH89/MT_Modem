using System.Text;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        //************ Terminal Display Functions ************************


        private void display_init()
        {
            int x, y;

            //
            // 100 rows by 80 columns. 0xa in position maxCol (81) causes textbox to start newline
            //
            for (int j = 0; j < 100; j++)
            {
                int k;
                display[j * maxCol] = (byte)((j / 10) % 10 + 0x30);     // For testing purposes
                display[j * maxCol + 1] = (byte)(j % 10 + 0x30);
                for (k = 0; k < numCol; k++)            // set start at 2 if testing screen
                    display[j * maxCol + k] = 0x20;
                if (j < maxRow-1)
                    display[j * maxCol + k] = 0X0a;
            }
            h19Term.Text = Encoding.UTF8.GetString(display);
            h19Term.SelectionStart = h19Term.Text.Length;
            h19Term.ScrollToCaret();
        }
 
        private void displayChar(byte ch)
        {
            // display = 100 x 80, Start writing on line 75. First 75 lines for future scroll capabilty
            switch (ch)
            {
                case CR:
                case LF:
                    cursorX = 0;
                    cursorY++;
                    break;
                case DEL:
                    int j;
                    for (j = cursorY * maxCol + cursorX; j < cursorY * maxCol+numCol; j++)
                        display[j] = display[j + 1];
                    display[j] = 0x20; // does not wrap text from next line
                    break;

                case BS:
                    bool chgXY = false;
                    if (cursorX > 0 && cursorX < numCol )
                    {
                        cursorX--;
                        chgXY = true;
                    }
                    else
                    {
                        if (cursorY > 75)
                        {
                            cursorY--;
                            cursorX = numCol-1;
                            chgXY = true;
                        }
                    }
                    if(chgXY)
                        display[cursorY * maxCol+ cursorX] = SP;
                    break;
                default:
                    display[cursorY * maxCol+ cursorX] = ch;
                    cursorX++;
                    if (cursorX == numCol)
                    {
                        cursorX = 0;
                        cursorY++;
                    }
                    break;
            }
            int p1 = h19Term.GetFirstCharIndexFromLine(cursorY);
            h19Term.Select(p1,  maxCol);        // Select line of text in RTB
            byte[] temp = new byte[maxCol];
            Buffer.BlockCopy(display, p1, temp, 0, maxCol);
            h19Term.SelectedText = Encoding.UTF8.GetString(temp);
            h19Term.SelectionStart = cursorY * maxCol + cursorX;        // set the focus for the cursor
            //h19Term.ScrollToCaret();          // causes line jump in terminal window
            //
            // Update cursor display
            updateCursorDisplay();
            
        }
        private void updateCursorDisplay()
        {
            Invoke(new Action(() => {
                cursorBox.Text = cursorX.ToString() + "x" + (cursorY - 75).ToString();
            }));
        }
       
    }
}