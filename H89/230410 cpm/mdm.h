/******************************************************/
/* Define Statements for MT Modem
/* Created: 6/23/89
/* Last Modified: 7/4/89 - added XON, XOFF, BLKSIZE
/******************************************************/

#undef HDOS

#define H89
#define SMALLZ

#ifdef H89
#undef SMALLZ
#endif

#define FALSE 0
#define TRUE 1
#define DEBUG TRUE

#define NUL  0               /* XMODEM constants */
#define SOH  1
#define STX  2
#define EOT  4
#define ACK  6
#define NAK  0x15
#define CAN  0x18
#define XON  0x11
#define XOFF 0x13
#define SUB  0x1A

#define TAB  9               /* Generic constants */
#define LF  0x0a
#define CR  0x0D
#define BS  0x08
#define Space ' '
#define DEL 0x7F
#define ESC 0x1b

#ifdef H89
#define version "H-89 1.0 17 Mar 2023"
#define MODDATP 0D8H         /* H89 Serial port constants */
#define BASEPORT 0xD8        /* for C code */
#define CONPORT 0E8H         /* H-89 Console Port */
#define CAPKEY 'P'           /* Blue Func Key */
#define CTLPRT 0DH          /* Assembly language defintions */
#define H88CTL 0F2H
#define SPDBIT 10H
#define NSPDBIT 0EFH
#define B4800  0x18
#define B9600  0x0c
#define B19200 0x06
#define B38400 0x03
#define B57600 0x02
#define TICCNT 0x0B
#define TICKSEC 500          /* 15 ticks/sec SmallZ80, 500 H89 */
#define BLKSIZE 1024
#endif

#ifdef SMALLZ
#define version "Small Z80 1.0 3 Mar 2023"
#define TICCNT 0x0E
#define TICKSEC 15          /* 15 ticks/sec SmallZ80, 500 H89 */
#define MODDATP 078H         /* Small Z80 Serial port constants */
#define BASEPORT 0x78        /* for C code */
#define CAPKEY 'O'           /* F1 Func Key */
#define B4800  0xC0
#define B9600  0x60
#define B19200 0x30
#define B38400 0x18
#define B57600 0x10
#define B115000 0x08
#define BLKSIZE 1024
#endif

#ifdef HDOS
#define version "H-89 HDOS 1.0 17 Mar 2023"
#define MODDATP 0D8H         /* H89 Serial port constants */
#define BASEPORT 0xD8        /* for C code */
#define CONPORT 0E8H         /* H-89 Console Port */
#define CAPKEY 'P'           /* Blue Func Key */
#define CTLPRT 0DH          /* Assembly language defintions */
#define H88CTL 0F2H
#define SPDBIT 10H
#define NSPDBIT 0EFH
#define B4800  0x18
#define B9600  0x0c
#define B19200 0x06
#define B38400 0x03
#define B57600 0x02
#define TICCNT 0x201B
#define TICKSEC 500          /* 15 ticks/sec SmallZ80, 500 H89 */
#define BLKSIZE 256
#endif



#define lastbyte 128
#define timeout   -1
#define errormax  10
#define retrymax  10
#define BLOCK  16384        /* must be multiple of 1024 */
#define TBLOCK  4096        /* must be multiple of 1024 */
#define INITSZ 128


