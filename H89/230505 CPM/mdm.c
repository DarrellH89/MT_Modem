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
/**************************************************/

#include "mdm.h"
#include "printf.h"
/* comment to fix strange compiler behavior */

unsigned *ticks;
int baudrate ;         /* global baud rate divisor */
char port;           /* current modem port */
char fastFlag;          /* FLag to slow down for serial port reads */
int baud;      /* current modem speed */
char *rcvbuf, *rcvptr;
char statline[80];   /* status line data */

extern int tranSize;
extern char xyModem;
extern char Curstr[] ;           /* cursor string */
extern char RetCur[] ;
extern char SaveCur[] ;
extern char ClrEOL[] ;
extern char ClrScr[] ;
extern char On25[] ;
extern char Off25[] ;

/*********************  Main Modem    *********************************/



main()
  {
  int j, fp;
  char buf[256];
  static int ok;
  char option;

  if( H89FLAG == 1)
      slow();
  init() ;

  do
    {
      switch( option = GetOption() )
      {
        case 'O': SetBaud() ;         break ;
        case 'E': Terminal(option);  break;
        case 'T': Terminal(option);  break;
        case 'C': Conference()       ;  break;
        case 'R': ReadFile()  ;      break;
        case 'S': SendFile()  ;      break ;
        case 'Y': SetYmodem() ;      break;
        case 'H': Hangup();     ;   break ;
        case 'D': DebugOption(); break;
        case '4': Fast(); break;
      }
    }
    while (option != 'X') ;
  puts( Off25 ) ;
  if(H89FLAG == 1 )
      fast();
  return 0;
  }

/*********************    GetOption ***************************/

int GetOption()
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
/*    printf("    C - conference terminal mode\n");
*/
    printf("  H - hang up the phone\n");
    printf("  O - option configuration (%d, %x)\n", baud, port);
    printf("  D - Debug Speed options\n");
    printf("  X - exit to system\n");
    printf("\nwhich ? ");
    do ch = toupper( GetCon() );     /* direct console I/O */
    while (ch == 0);
    putchar('\n');
    return ch;
    }

/*********************    DebugOption ***************************/
DebugOption()
  {
  char ch[2] ;

  ch[1] = 0;
  do  {
      printf("\n  2 - Slow Speed Operation\n");
      printf("    4 - Fast Speed Operation\n");
      printf("    S - Only slow for Serial Reads\n");
      printf("    0 - Exit Menu\n");
      printf("    Which?    \n");
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
#ifdef HDOS
  set_cline();
#endif
  }

/********************** Conference **********************/
/* Conference mode */

#define  ScrDiv  21        /* location oif the screen divide line*/
static int line25;
/* ********* co in char *******/
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
  static int mx, my,  tx, ty ;          /* Modem and Terminal X,Y coord */
  static int j ;

  puts( ClrScr );
  tx = 1 ;
  ty = 25 ;
  line25 = FALSE ;
  gotoXY( 1,3 );     /* start position */
  printf("\nUse ctrl-E to exit Conference mode.\n\n");

  do{
    co_inchar() ;        /* check for modem input */
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
    if ( (t == CR)||(tx > 80) )    /* Need to send buffer to host? */
        {
        for (j = 1; j < tx; j++ )    /* send chars from 1 to tx */
        {
        Send( cobuff[j] ) ;
        co_inchar() ;       /* check for input after each char */
        }
        tx = 1 ;
        if ( t != CR )     /* tiggered by line length, so must send CR */
        Send( CR ) ;
        }
    }
    } while (t != 5);
  gotoXY( 1,25) ;
  puts( ClrEOL ) ;
  puts( Off25 );
  puts( RetCur ) ;
#ifdef HDOS
  set_cline();
#endif
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

    rcvbuf = alloc( BLOCK + 256 );  /* 128 for Ymodem filename, 256 HDOS */
    rcvptr = rcvbuf;


    if( rcvbuf == -1)
    {
    printf("Not enough memory - TERMINATED\n");
    exit();
    }
    Setup( baudrate );     /* default of Originate 1200 baud */
    strcpy( statline, "19200       8N1 D8");
    Stat25();
#ifdef HDOS
   set_cline() ;
#endif
#ifdef H89
    printf("%cv", ESC );    /* set terminal wrap mode */
#endif
    }

/************************ Write Buffer ************************/

writebuf( fp )
  int fp ;
  {
  static int j ;

  Send( XOFF ) ;
  while ( ((rcvptr - rcvbuf) % BLKSIZE) != 0 )
      *rcvptr++ = 0 ;                 /* flush out to an even 128 size */
  printf("\nWriting %d bytes.\n", (rcvptr-rcvbuf));
  j = write( fp, rcvbuf, (rcvptr - rcvbuf) );
  if ( j != (rcvptr - rcvbuf) )
      printf("\nFile Write ERROR! Only %d bytes written\n", j);
  rcvptr = rcvbuf ;
  Send( XON );
  }


/*********************    Terminal ***************************/
Term()
{
#asm
DataPort  equ 0d8h
MCntPort  equ DataPort + 4
LCntPort  equ DataPort + 3
StPort      equ DataPort + 5
MStPort   equ DataPort + 6
IStPort   equ DataPort + 2   ;        /* Interrupt ID Port */

TxMask     equ 20h      ;        /* Transmit Empty Buffer mask */
RXMask     equ 1

    .z80
    di
Loop:    call GetData          ; check if computer sent anything & transmit
    jr   z, console
    call pcon
console:
    call  GetCon
    ld    a,l
    and    a
    jr   z, loop
    ld   b, 5         ; ^E
    cp   b
    jr   z, cleanup
    call  DataOut
    jr   Loop          ; Loop forever

cleanup:
    ei
    ret

GetData:in    a,( StPort )
    and    RxMask
    ret    z         ; No data so return
    in    a,(DataPort)
    and    a        ; set the flags
    ret


;/*** Data Out ***/
;/* Send a char to the modem */

DataOut:push    af
DataO1: in    a,( StPort )      ; Check Status
    and    TxMask
    jr    z,DataO1      ; Loop until Ready
    pop    af          ; Get the data
    out    ( DataPort ),a      ; ans send it
    ret

;/*************** putcon *****************************/
;/* Put char c loaded in A out port e8h  */

pcon:
    push    af
putc1:    in    a,(CONPORT+5)    ; check console status
    and    20h
    jr    z,putc1     ; loop until console ready
    pop    af
    out    (CONPORT),a
    ret
;************************
; getcon - gets char from console

gcon:
    in    a,(CONPORT+5)    ; check console status
    and    1
    ret    z            ; return 0 if no char
    in    a, (CONPORT)
    and    a        ; set the flags
    ret
    
    .8080
#endasm 

    
}
Terminal( op )
    char op;
    {
    char t, m, n, filename[20], ctlT ;
    static int capture,    /* capture text flag */
           fp ,       /* file pointer */
           j, funcKey;
    unsigned cnt ;
    int cts, rcnt;

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
    do    {
    if ( (m = rcvchar()) != -1)   /* Check port for serial data */
        {
        if ( capture )
        {
        *rcvptr++ = m ;
        if ( (rcvptr - rcvbuf) == BLOCK )
            writebuf() ;
        }
        m = m & 0x07f ;    /* mask nasty bits for display */
        putchar( m );
        if( op == 'E')    /* Echo back */
        Send( m );
        }
    if ( ((t = GetCon()) != 0) && (t != 5) )  /* Console data */
        {
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
            printf(" Set CTS, Start Data\n");
            cts = 0;
            }
          else
            {
            ResetDTR();
            printf(" Reset CTS, Stop Data\n");
            cts = 1;
            }
        if((funcKey == 0)&&(t != ctlT))
        {
        Send( t );
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
       writebuf() ;       /* write the buffer */
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
      *fp = fopen( filename, "ub" );
        /* add to end of file if it exists else open new file
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

/****** Interrupt functions *****/
ei()
{
#asm
    ei
#endasm
}

di()
{
#asm
    di
#endasm
}
