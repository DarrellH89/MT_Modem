/* MT Modem include file routines */
/* uses port in Setup() and DTR and CTS functions */

#include "printf.h"
#include "mdm.h"

/*
#ifdef HDOS
#include "hdos.h"
#endif
*/

/* These globals are used in other modules   */
/* Modify .MAC file to add names so the EXTRN command can find them */

extern unsigned *ticks;
extern char statline[] ;
extern int baudrate ;        /* global baud rate divisor */
extern int baud;               /* global baud speed */
extern char port;         /* Serial port */
extern char fastFlag;
int tranSize = 1024;        /* Default packet size */
char xyModem = 'X';        /* Default Xmodem */

#ifdef SMALLZ
char Curstr[] = { ESC, '[', ' ',';', ' ','f', 0 } ;        /* cursor string */
char ClrEOL[] = { ESC, '[','0','K', 0 } ;         /* Clear to End of Line
char ClrScr[] = { ESC, '[', '2', 'J', 0 }  ;           /* Clear the screen *
char SaveCur[] = { ESC, '7', 0 }  ;      /* Save the current cursor position */
char RetCur[] = { ESC, '8', 0 }    ;      /* Restore to previous saved pos. */
char On25[] =    { ESC, 'x', '1', 0 } ;         /* Enable 25th line  */
char Off25[] =    { ESC, 'y', '1', 0 }  ;      /* Disable 25th line  */
#else
char Curstr[] = { ESC, 'Y', ' ', ' ', 0 } ;        /* cursor string */
char ClrEOL[] = { ESC, '[','K', 0 } ;             /* Clear to End of Line */
char ClrScr[] = { ESC, '[','E', 0 }  ;             /* Clear the screen */
char SaveCur[] = { ESC, 'j', 0 }  ;      /* Save the current cursor position */
char RetCur[] = { ESC, 'k', 0 }    ;      /* Restore to previous saved pos. */
char On25[] =    { ESC, 'x', '1', 0 } ;         /* Enable 25th line  */
char Off25[] =    { ESC, 'y', '1', 0 }  ;      /* Disable 25th line  */
#endif



/*************** SetBaud *********************/
SetBaud()
    {
    int i ;
    char *ptr, *ptrb ;

    ptr = "\0";
    ptrb = "D8";
    puts( SaveCur ) ;          /* save the cursor position */
    puts( On25 ) ;          /* enable the 25th line */
    gotoXY( 1,25 ) ;        /* go to line 25 */

    printf("%s%s%cK", "Baud? 1) 19200 2) 9600 3) 4800 4) 1200 ",
              "  Port? 5) D0 6) D8 7) E0", ESC );
    do i = GetCon() ;
    while (i == 0) ;
    puts( Off25 );
    puts( RetCur );
    switch (i-'0') {
    case 1: baudrate = B19200 ;
        ptr = "19200" ;
        baud = 19200;
        break ;
    case 2: baudrate = B9600 ;
        ptr = "9600" ;
        baud = 9600;
        break ;
    case 3: baudrate = B4800 ;
        ptr = "4800" ;
        baud = 4800;
        break ;
    case 4: baudrate = 0x60 ;
        ptr = "1200" ;
        baud = 1200;
        break ;
    case 5: port = 0xd0  ;
        ptrb = "D0";
        break ;
    case 6: port = 0xd8;
        ptrb = "D8";
        break ;
    case 7: port = 0xe0;
        ptrb = "E0";
        break ;
    }
    if(ptr != 0)
    {
    strcpy( statline, ptr );
    for( i = strlen(ptr); i < 14; i++)
        statline[i] = ' ';
    statline[ 13 ] = '8' ;
    statline[ 14 ] = 'N' ;
    statline[ 15 ] = '1' ;
    statline[ 16 ] = 0 ;
    Setup( baudrate );
    }
     statline[ 16 ] = ' ' ;
     statline[ 17 ] = *ptrb++;
     statline[ 18 ] = *ptrb;
     statline[ 19 ] = 0 ;
     SetPort();


/*
for(i=0; i <17; i++)
    printf("%d.c %c hex %x\n", i,statline[i], statline[i]);
i = getchar();
*/
    Stat25() ;             /* write the status on the 25th line */
    return 0 ;
    }
/********************* SetPort *********************/
/* modifies code to update MODDATP value
   osport - Ostatus = port + 5
   isport - Istatus = port + 5
   sdport - Send    = port
   rcport - rcvchar = port
*/

SetPort()
  {
#asm
  .z80
  ld       a,(port)
  ld       (sdport+1), a
  ld       (rcport+1), a
  add       a, 5
  ld       (osport+1), a
  ld       (isport+1), a
  .8080
#endasm
  }


/********************* SetYmodem *********************/
setYmodem()
    {
    char ch ;

    do
    {
    printf("\n");
    printf("Options? 1) Transfer Block 128 bytes\n");
    printf("     2) Transfer Block 1024 bytes\n");
    printf("     3) Xmodem\n");
    printf("     4) Ymodem\n");
    printf("     0) Exit\n",);
    printf("    ?: ");

    do ch = GetCon() ;
       while (ch == 0) ;
    putchar('\n');
    switch (ch)
       {
       case '1':
            tranSize = 128;
            printf("Block size: %d\n",tranSize);
            break ;
       case '2':
            tranSize = 1024;
            printf("Block size: %d\n",tranSize);
            break ;
       case '3':
            xyModem = 'X';
            printf("Set for Xmodem\n");
            break ;
       case '4':
            xyModem = 'Y';
            printf("Set for Ymodem\n");
            break ;
       }
    } while(ch != '0')   ;

    }
/********************** FlushCon ******************
/* flush console
/* intput: none
/* output: void
*/
FlushCon()
    {
    int ch ;

    while((ch=GetCon())!= 0);
    }

/*********************    Set DTR ***************************/

SetDTR()
  {
#asm
    .Z80
     ld   a,(port)
     ld   c, 4
     add  a,c
     ld   c,a
     ld   a,1
     out  (c), a
     .8080
#endasm
  }
/*********************    Reset DTR ***************************/

ResetDTR()
  {
#asm
    .Z80
     ld   a,(port)
     ld   c, 4
     add  a,c
     ld   c,a
     ld   a,0
     out  (c), a
     .8080
#endasm
  }
/*********************    Set CTS ***************************/
/* Allow Data */

SetCTS()
  {
#asm
    .Z80
     ld   a,(port)
     ld   c, 4
     add  a,c
     ld   c,a
     ld   a,2

     out  (c), a
     .8080
#endasm
  }
/*********************    Reset CTS ***************************/
/*  Stop Data */

ResetCTS()
  {
#asm
    .Z80
     ld   a,(port)
     ld   c, 4
     add  a,c
     ld   c,a
     ld   a,0
     out  (c), a
     .8080
#endasm
  }

/*********************    Output Status ***************************/

Ostatus()
  {
#asm
    .Z80
osport:     in      a, (MODDATP+5)
     and   20h
     ld    l,a
     ld    h,0
    .8080
#endasm
  }
/*********************    Slow  ***************************/

Slow()
  {
#asm
    .Z80
    ld     a,(CTLPRT)
    and  NSPDBIT
    ld     (CTLPRT), a
;   OUT  (H88CTL), A
    .8080
#endasm
   fastFlag = 0;
  }
/*********************    Fast ***************************/

Fast()
  {
#asm
    .Z80
    ld     a,(CTLPRT)
    OR     SPDBIT
    ld     (CTLPRT), a
 ;  OUT  (H88CTL), A
    .8080
#endasm
  fastFlag = 1;
  }
/*********************    Input Status ***************************/
Istatus1()
  {
#asm
    .Z80
xisport:
    in       a,(MODDATP+5)
    and    3
    ld       l,a
    ld       h,0
    .8080
#endasm
  }

/*********************    Send ***************************/

Send( ch )
    char  ch;
    {
    while ( !Ostatus() );   /* wait until ready */
#asm
    .Z80
     ld   hl,2
     add  hl,sp
     ld   a,(hl)
sdport:
     out   (MODDATP),a
     .8080
#endasm
    }

/*********************    rcvchar ***************************/

rcvchar()
  {
#asm
    .z80
isport:
    in       a,(MODDATP+5)
    and    1
    ld       hl, -1
    ret    z
rcport:
    in      a, (MODDATP)
    ld      h,0
    ld      l,a
    .8080
#endasm
    }



/*********************    Readline ***************************/

Readline( cnt )
  unsigned cnt;
  {
  int m ;

  cnt = cnt * TICKSEC  ;  /* H89 = 500 number of 2ms in a
                 second plus *ticks
                 /* Small Z80 tick = 65.2 ms, 16 /sec */
  *ticks = 0;
  m  = -1;             /* error value */
  while( cnt > *ticks )
      if( (m = rcvchar()) != -1)
      break;
  return m  ;
  }


/*********************    GetCon ***************************/

GetCon()
    {
/*  HDOS */
#ifdef HDOS
#asm
    SCALL    MACRO    TYPE
        DB    377Q,TYPE
    ENDM

    scall    1    ; check console stat, return 0 if not ready
    lxi    h,0
    rc
    mov    l,a    ; Had a char, so return it in HL
#endasm
#else
/* CP/M    */
#asm
    .z80
    ld    e,0ffh    ; check for input
    ld    c,6    ; BDOS function 6
    call    5
    ld    hl,0    ; make sure hl = 0
    ld    l,a    ; Make L = A, 0 if no char
    .8080
#endasm
#endif
    }

/*********************    SendText ***************************/
SendText( str )
  char *str;
  {
  while ( *str != 0 )
      Send( str++ );
  }


/******************* Setup ********************/




Setup( baud )
int baud ;        /* desired baud rate divisor */
{

#ifdef HDOS
set_cchar();
#endif

/**/
#asm
    .z80
    ld   a,(port)
    ld   b,a          ; save port in b
    ld   c, 1         ; port + 1
    add  a,c
    ld   c,a
    ld     hl,2          ; set HL to point to the desired baud
    add    hl,sp          ;   rate pushed onto the stack
    di              ; Kill interrupt mode
    xor    A          ;
    out    (c),a      ; interrupt enable register
    inc    c      ; port + 2
    out    (c),a      ; interrupt enable register
    ld    a,83H          ; Set divisor (baud rate) latch in
    inc    c          ; MODDATP+3
    out    (c),a         ; line control register
    ld    a, (hl)
    ld    c, b        ; port
    out    (c),a      ; Set LS divisor
    inc    hl
    ld    a,(hl)
    inc    c      ; port + 1
    out    (c),a      ; Set MS divisor
    ld     a,3        ; set to 8N1
    inc    c          ; port+2
    inc    c          ; port+3
    out    (c),a         ; modem control register
    inc    c           ; MODDATP + 4
    ld    a,1
    out    (c), a           ; set DTR on
    ei
    ld    c,b       ;  port
    in    a,(c)     ;(MODDATP)      ; clear any garbage
    in    a,(c)     ;a,(MODDATP)
    ld    hl,0          ; Return 0
;     ret
    .8080
#endasm
/**/


/* original code
#asm
    .z80
    ld     hl,2          ; set HL to point to the desired baud
    add    hl,sp          ;   rate pushed onto the stack
    di              ; Kill interrupt mode
    xor    A          ;
    out    (MODDATP+1),a      ; interrupt enable register
    ld    a,80H          ; Set divisor (baud rate) latch in
    out    (MODDATP+3),a      ; line control register
    ld    a,(hl)
    out    (MODDATP), a      ; set the baud rate LS divisor
    inc    hl          ; point to the next byte
    ld    a,(hl)          ; set the MS divisor
    out    (MODDATP+1),a
    ld    a,3          ; set to 8N1
    OUT    (MODDATP+3),a
    ld    a,1          ; set DTR on
    out    (MODDATP+4),a
    ei
    in    a,(MODDATP)      ; clear any garbage
    in    a,(MODDATP)
    ld    hl,0          ; Return 0
    ret
    .8080
#endasm
/**/
}



/*************** putcon *****************************/
/* Put char c out port e8h  */

putcon( c )
  char c ;
  {
  putc(c);
/*
#asm
    .z80
    ld    hl,4        ; get the pointer to the string
    call    mtoffset
putc1:    in    a,(CONPORT+5)    ; check console status
    and    20h
    jr    z,putc1     ; loop until console ready
    ld    a,(hl)
    out    (CONPORT),a
putce:    ld    hl,0        ; return success
;     ret
    .8080
#endasm
*/
  }

/*************** puts *****************************/
/* Put a string S to port e8h until 0 */

puts( s )
  char *s ;
  {
/*  printf("%s", s);
/*/
#asm
    .z80
    ld    hl,4        ; get the pointer to the string
    call    mtoffset
puts1:    in    a,(CONPORT+5)    ; check console status
    and    20h
    jr    z,puts1     ; loop until console ready
    ld    a,(hl)
    or    a        ; check for 0
    jr    z,putse     ; done
    out    (CONPORT),a
    inc    hl        ; point to next
    jr    puts1
putse:    ld    hl,0        ; return success
;     ret
;
mtoffset::
    add    hl,sp        ; HL contains offset to value in stack
    ld    a,(hl)        ; get LS byte
    inc    hl        ; point to MS byte
    ld    h,(hl)        ; get MS byte
    ld    l,a        ; load LS byte into L
    ret
    .8080
#endasm
  }



/*************** goto XY *****************************/
gotoXY( x,y )
   int x, y;        /* x = col, y = row     */
    {
/*    printf("%c[%d;%df",ESC,x,y);
*/
#asm
        .z80
        ld      hl,6            ; get x
        call    mtoffset
        ld      de,31           ; add video offset
        add     hl,de
        ld      a,l             ; save the x value
        ld      (curstr+3),a
        ld      hl,4            ; get y
        call    mtoffset
        ld      de,31
        add     hl,de           ; add video offset for y
        ld      a,l
        ld      (curstr+2),a
        ld      hl,curstr       ; get address of XY cursor string
        push    hl              ; put on stack for puts()
        call    puts
        pop     bc              ; fix stack

    ld    hl,0        ; return 0
    ret
    .8080
#endasm
}
