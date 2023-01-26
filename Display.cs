using System.Text;
using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace MT_MDM
{
    public partial class MtMdm : Form
    {
        //************ Terminal Display Functions ************************


        private void display_init()
        {
            int x, y;

            //
            // 100 rows by 80 columns. 0xa in position 81 causes textbox to start newline
            //
            for (int j = 0; j < 100; j++)
            {
                int k;
                display[j * 81] = (byte)((j / 10) % 10 + 0x30);
                display[j * 81 + 1] = (byte)(j % 10 + 0x30);
                for (k = 2; k < 80; k++)
                    display[j * 81 + k] = 0x20;
                if (j < 99)
                    display[j * 81 + k] = 0X0a;
            }
            h19Term.Text = Encoding.UTF8.GetString(display);
            h19Term.SelectionStart = h19Term.Text.Length;
            h19Term.ScrollToCaret();
        }
        //private void displayChar1(byte ch)
        //{
        //    // Font 8 x 10
        //    //char ch;
        //    int charSize = charWidth*2;

        //    lock (bm)
        //    {
        //        if (ch > 0x1f && ch < 0x80)
        //        {
        //            int cx = cursorX * (charWidth * 2-2);
        //            int cy = cursorY * (charHeight * 2 + 8);
        //            int x, y, ptrFont, t;
        //            int mask = 128;


        //            ptrFont = ch * 16;
        //            Color newColor = Color.FromArgb(0, 250, 0);
        //            for (y = 0; y < 10; y++)
        //            {
        //                mask = 128;
        //                for (x = 0; x < 8; x++)
        //                {

        //                    t = h19.h19Font[ptrFont];
        //                    if ((h19.h19Font[ptrFont] & mask) > 1)
        //                    {
        //                        bm.SetPixel(cx + x*2, cy + y, newColor);
        //                        bm.SetPixel(cx + x*2 + 1, cy + y, newColor); 
        //                        bm.SetPixel(cx + x*2, cy + y+1, newColor);
        //                        bm.SetPixel(cx + x*2 + 1, cy + y + 1, newColor);
        //                    }
        //                    //cx += 2;
        //                    mask = mask / 2;
        //                }
        //                cy += 2;
        //                ptrFont++;
        //            }
        //            cursorX ++;
        //            if (cursorX == numCol )
        //            {
        //                cursorX = 0;
        //                cursorY ++;
        //            }

        //            termH19.Image = bm; 
        //            Invoke(new Action(() => { 
        //                cursorBox.Text = cursorX.ToString() + "x" + cursorY.ToString(); 

        //                }));              

        //        }
        //    }
        //}
        private void displayChar(byte ch)
        {
            // Font 8 x 10
            //char ch;
            //int charSize = charWidth * 2;
            // display = 100 x 80
            switch (ch)
            {
                case CR:
                case LF:
                    cursorX = 0;
                    cursorY++;
                    break;
                case DEL:
                case BS:
                    if (cursorX > 0)
                        cursorX--;
                    else
                    {
                        cursorX = 80;
                        cursorY--;
                    }
                    display[cursorY * 81 + cursorX] = SP;
                    break;
                default:
                    display[cursorY * 81 + cursorX] = ch;
                    cursorX++;
                    if (cursorX > 79)
                    {
                        cursorX = 0;
                        cursorY++;
                    }
                    break;
            }
            h19Term.Text = Encoding.UTF8.GetString(display);
            h19Term.SelectionStart = cursorY * 81 + cursorX;//h19Term.Text.Length;
            h19Term.ScrollToCaret();
            Invoke(new Action(() => {
                cursorBox.Text = cursorX.ToString() + "x" + (cursorY - 75).ToString();
            }));



        }
       
    }
}