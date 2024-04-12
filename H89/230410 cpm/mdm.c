/**************************************************
/* Modem for HDOS and CP/M
/* Author: Darrell Pelan
/* written for Software Toolworks C80
/* Created 3/6/87
/* Last Modified: 12/20/2022 Modified for Small Z80, Added Ymodem
/*    6/26/89 - add Send File Xmodem mode
/*    6/19/89 - add Conference mode
/*    4/10/89 - ver 0.6, recv XMODEM, Terminal
/*
**************************************************/

#include "mdm.h"
#include "printf.h"
#include "scanf.h"

static char option ;
char *rcvbuf, *rcvptr;
char statline[80] ;             /* status line data; baud rate     0-15
                                                     capture file 17-30 */
unsigned *ticks;
int baudrate ;           /* global baud rate divisor */
char port;             /* current modem port */
char fastFlag;        /* FLag to slow down for serial port reads */
int baud;         /* current modem speed */

extern int tranSize;
extern char xyModem;
extern char Curstr[] ;         /* cursor string */
extern char RetCur[] ;
extern char SaveCur[] ;
extern char ClrEOL[] ;
extern char ClrScr[] ;
extern char On25[] ;
extern char Off25[] ;

test()
  {
  int x, y;
  x = 1;
  do
    {
    printf(" X Y ");
    scanf("%d", &x);
    scanf("%d", &y);
    gotoXY( x, y);
    } while ( x!=0);
  printf("%cz", ESC);
  puts( SaveCur );
  gotoXY(1,25);
  printf("%s Hello %s", On25, Off25);
  puts(RetCur);
  x = getchar();
  }


 /************************  Main Modem  *********************************/
main()
  {
  static int ok;
/*
  int op1, op2;
  test2(&op1, &op2);
  printf("asm test2 %x %x\n", op1, op2);
  test3(&op1, &op2);
  printf("asm test3 %x %x\n", op1, op2);
*/
  init() ;
  do
    {
    switch( option = GetOption() )
        {
        case 'O': SetBaud() ;        break ;
        case 'E': Terminal(option);  break;
        case 'T': Terminal(option);  break;
        case 'C': Conference()     ;  break;
        case 'R': ReadFile()  ;      break;
        case 'S': SendFile()  ;      break ;
        case 'Y': SetYmodem() ;      break;
        case 'H': Hangup();      ;   break ;
        case 'D': DebugOption(); break;
        case '4': Fast(); break;
        }
    }
    while (option != 'X') ;
  puts( Off25 ) ;
  Fast();
  }

/*********************  GetOption ***************************/

GetOption()
    {
    char ch;

/*    printf("%cE", ESC );   /* clear the screen */
    FlushCon();
    printf("\n\nModem, %s\n",version);
    printf("Options:\n\n");
    printf("  R - receive a file\n");
    printf("  S - send a file\n");
    printf("  Y - X/Ymodem Options (Block %d, ", tranSize);
    if( xyModem == 'X')
        printf("Xmodem)\n");
      else
        printf("Ymodem)\n");
    printf("  T - terminal mode\n");
    printf("  E - terminal mode with Echo\n");
/*    printf("  C - conference terminal mode\n");
*/
    printf("  H - hang up the phone\n");
    printf("  O - option configuration (%d, %x)\n", baud, port);
    printf("  D - Debug Speed options\n");
    printf("  X - exit to system\n");
    printf("\nwhich ? ");
    do ch = toupper( GetCon() );     /* direct console I/O */
        while (ch == 0);
    printf("%c\n", ch );
    return ch;
    }

/*********************  DebugOption ***************************/
DebugOption()
  {
  char ch[2] ;

  ch[1] = 0;
  do  {
      printf("  2 - Slow Speed Operation\n");
      printf("  4 - Fast Speed Operation\n");
      printf("  S - Only slow for Serial Reads\n");
      printf("  0 - Exit Menu\n");
      printf("  Which?  \n");
      do
          ch[0] = toupper(GetCon());
          while (ch[0] == 0 );
      } while( index("24S0", ch) == -1);
  switch( ch[0] )
      {
      case '2': Slow(); fastFlag = 0; break;
      case '4': Fast(); fastFlag = 1; break;
      case 'S': if(fastFlag == 0)
                    {
                    fastFlag++;
                    Fast();
                    }
                  else
                    {
                    fastFlag = 0;
                    Slow();
                    }
                printf("FastFlag %d\n", fastFlag);
                break;
      }
  return 0;
  }
/********************** Statline **********************/
/* Write Status line on 25th line; uses global "statline" */

Stat25()
  {
  puts( SaveCur ) ;
  puts( On25 ) ;
  gotoXY( 1,25 );
          /* Reverse Video, statline, stop reverse video, Erase to EOL */
  printf("%c%cp%s%cq%cK", CR, ESC, statline, ESC, ESC  );
  puts( RetCur ) ;
  }

/********************** Conference **********************/
/* Conference mode */

#define  ScrDiv  21         /* location of screen dividing line */
static int line25 ;         /* Line 25 active flag */

/******************* co in char ******************/
/* check for input from modem and display it */

co_inchar()
  {
  register int m ;

  if ( Istatus() != 0 )
      {
      m = rcvchar() & 0x7F;
      if (line25 == TRUE)
          {
          line25 = FALSE ;
          puts( RetCur ) ;
          }
      if (m != ESC )
          putchar( m );
      }
  }


Conference()
  {
  static char t, cobuff[100] ;
  static int mx, my,  tx, ty ;            /* Modem and Terminal X,Y coord */
  static int j ;

  puts( ClrScr );
  tx = 1 ;
  ty = 25 ;
  line25 = FALSE ;
  gotoXY( 1,3 );     /* start position */
  printf("\nUse ctrl-E to exit Conference mode.\n\n");

  do{
    co_inchar() ;               /* check for modem input */
    if ( ((t = GetCon()) != 0) && (t != 5) )
        {
        if (line25 == FALSE)
            {
            puts( SaveCur );
            line25 = TRUE ;
            puts( On25 );
            }
        if ( t == BS )
            {
            if (tx > 1)
               gotoXY( --tx,ty ) ;    /* erase current char */
            putchar( ' ' );
            }
          else
            {
            gotoXY( tx, ty );
            if( tx == 1 )
                puts( ClrEOL ) ;
            cobuff[ tx++ ] = t ;
            }
        putchar( t );
        if ( (t == CR)||(tx > 80) )     /* Need to send buffer to host? */
            {
            for (j = 1; j < tx; j++ )   /* send chars from 1 to tx */
                {
                Send( cobuff[j] ) ;
                co_inchar() ;      /* check for input after each char */
                }
            tx = 1 ;
            if ( t != CR )       /* tiggered by line length, so must send CR */
                Send( CR ) ;
            }
        }
    } while (t != 5);
  gotoXY( 1,25) ;
  puts( ClrEOL ) ;
  puts( Off25 );
  puts( RetCur ) ;
  }

/******************** Hangup *****************************/

Hangup()
  {
  ResetDTR();
  }


/********************** Init ****************************/
/* initialize the system and globals */

init()
    {
    ticks = TICCNT;
    baudrate = B19200 ;
    baud = 19200;
    port = BASEPORT;
    fastFlag = 1;

#ifdef HDOS
    set_cchar() ;
#else
    printf("%cv", ESC );        /* set terminal wrap mode */
#endif
    rcvbuf = sbrk( BLOCK + 128 );  /* 128 for Ymodem filename */
    if( rcvbuf == -1)
        {
        printf("Not enough memory - TERMINATED\n");
        exit();
        }
    Setup( baudrate );   /* default of Originate 1200 baud */
    strcpy( statline, "19200       8N1 D8");
    Stat25();
    SetDTR() ;
/**/
    }

/************************ Write Buffer ************************/

writebuf( fp )
  int fp ;
  {
  static int j ;

  Send( XOFF ) ;
  while ( ((rcvptr - rcvbuf) % BLKSIZE) != 0 )
      *rcvptr++ = 0 ;                        /* flush out to an even 128 size */
  printf("\nWriting %d bytes.\n", (rcvptr-rcvbuf));
  j = write( fp, rcvbuf, (rcvptr - rcvbuf) );
  if ( j != (rcvptr - rcvbuf) )
      printf("\nFile Write ERROR! Only %d bytes written\n", j);
  rcvptr = rcvbuf ;
  Send( XON );
  }


/*********************  Terminal ***************************/

Terminal( op )
    char op;
    {
    char t, m, n, filename[20], ctlT ;
    static int capture,    /* capture text flag */
               fp ,        /* file pointer */
               j, funcKey;
    unsigned cnt ;
    int cts;

    ctlT = 0x14;    /* ^T   */
/*    setCTS();
*/
    cts = 0;
    capture = 0 ;
    funcKey = 0;
    fp = 0 ;
    printf("\nUse ctrl-E to exit terminal mode.\n\n");
    if (op == 'E')
        printf("Echo Mode\n");
    do  {
        j = Istatus() ;
        if ( j > 0 )   /* Check port for serial data */
            {
            m = rcvchar() ;
            if ( capture )
                {
                *rcvptr++ = m ;
                if ( (rcvptr - rcvbuf) == BLOCK )
                    writebuf() ;
                }
/*printf("%x ",m);
*/
            m = m & 0x07f ;     /* mask nasty bits for display */
            /*
            if(m>0x1f||m == 13||m==10)
            */
            putchar( m );
            if( op == 'E')      /* Echo back */
                Send( m );
            }
        if ( ((t = GetCon()) != 0) && (t != 5) )  /* Console data */
            {
/*  printf("%x ", t);
*/
            if( (funcKey == 0) && (t == ESC))
                {
                funcKey = 1;
                printf("Function 1\n");
                }
            if( (funcKey == 1) && (t == CAPKEY))
                {
                CapBuf(&capture, &fp);
                funcKey = 2;
                }
            if(t == ctlT)   /* ^T */
                if(cts)
                    {

SetDTR();
/*/                    SetCTS();
/**/
                    printf(" Set CTS, Start Data\n");
                    cts = 0;
                    }
                  else
                    {
ResetDTR();
/*/                    ResetCTS();
/**/
                    printf(" Reset CTS, Stop Data\n");
                    cts = 1;
                    }
            if((funcKey == 0)&&(t != ctlT))
                {
                Send( t );
                putchar(t);
                if( t == CR)
                    {
                    t = LF;
                    putchar(t);
                    Send (t);
                    }
                if ( capture )
                    {
                    *rcvptr++ = t ;
                    if ( (rcvptr - rcvbuf) == BLOCK )
                        writebuf() ;
                    if( t == CR)
                        {
                        *rcvptr++ = LF ;
                        if ( (rcvptr - rcvbuf) == BLOCK )
                            writebuf() ;
                        }
                    }
                }
            if(funcKey == 2)
                funcKey = 0;
            }
        } while (t != 5);
        if (capture)
            {
            writebuf() ;        /* write the buffer */
            fclose( fp );
           printf("\nClosing Capture File: %s\n", filename );
           }
    }

/***************** CapBuf ************
/* input: int cap flag, 0 or 1
/* ouput: filepointer
*/

int CapBuf( cap, fp)
  int *cap, *fp;
  {
  char filename[20];
  int j;

  if (*cap)    /* toggle the flag H89 F1 ESC S*/
      {
      *cap = 0 ;  /* off */
      if ( (rcvptr - rcvbuf) > 0 )
          {
          writebuf( *fp ) ;   /* write the buffer */
          fclose( *fp );
          printf("\nClosing Capture File\n" );
          statline[16] = 0 ;
          Stat25() ;
          }
      }
    else
      {
      *cap = 1 ;
      rcvptr = rcvbuf ;
      FlushCon();
      printf("\nCapture Buffer Filename? ");
      getline( filename, 20 ) ;
      printf("\n" );
      rcvptr = rcvbuf ;
      *fp = fopen( filename, "ub" );
                /* add to end of file if it exists else open new file            */
                /* Ask if they want to overwrite file */
      if(*fp > 0)
          {
          printf("Overwrite %s (Y)? ", filename);
          if(toupper(getchar()) == 'Y')
              {
              printf("\nOverwriting...\n");
              fclose(*fp);
              unlink(filename);
              if((*fp = fopen(filename, "wb")) == 0)
                  {
                  printf("File Open Error\n");
                  *cap = 0;
                  }
              }
          }
        else
          {
          printf("\nOpening new file\n");
          *fp = fopen( filename, "wb" );
          if ( fp == 0 )
              {
              printf("ERROR! - Can't open file %s\n", filename ) ;
              *cap = 0 ;
              }
          }
      if(*cap == 1)
          {
          j = strlen( statline );
          for( ; j<17; j++ )
              statline[j] = ' ' ;
          statline[ 17 ] = 0 ;
          strcat( statline, filename );
          Stat25() ;
          }
      }
  }
