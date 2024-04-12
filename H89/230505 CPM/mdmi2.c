
/* Author: Darrell Pelan
/* written for Software Toolworks C80
/* Created 7/27/89
/* Last Modified: 2/6/2023 - Added Ymodem
/*    6/26/89 - add Send File Xmodem mode
/*    6/19/89 - add Conference mode
/*    4/10/89 - ver 0.6, recv XMODEM, Terminal
/*
/*************************************************/

#include "printf.h"
#include "mdm.h"
/* buffer comment */

extern char *rcvbuf, *rcvptr;
char *rptr;
extern char statline[80] ;           /* status line data; baud rate 0-15
                             capture file 17-30 */
extern unsigned *ticks;
extern int tranSize;
extern char xyModem;


/*********************    ReadFile ***************************/

 ReadFile()
  {
  int fp;

  if(xyModem == 'Y')
      {
      printf("Waiting for Ymodem filename\n");
      while((fp = GetYmodem()) > 0)
      ReceiveIt(fp);
      }
    else
      ReadXmodem();

  }

/*********************    ReadXmodem ***************************/
/* input: none
/* output: none
/* asks for file name, opens file pointer, calls ReceiveIt to get file
*/

ReadXmodem()
  {
  char filename[20], c ;
  int fp, j, k;
  char ch;

  FlushCon();
  printf("\n\nFilename.Ext ? ");
  j = getline(filename, 16);
  for(k = 0; k < j; k++)
     filename[k] = toupper(filename[k]) ;

  if (strlen(filename) > 0)
      {
      fp = fopen( filename, "ub");
      if( fp > 0)
      {
      printf("\nOverwrite file %s (Y/N)? ", filename);
      ch = toupper(getchar()) ;
      if(ch == 'Y')
          {
          printf("\nOverwriting...\n");
          fclose(fp);

          unlink(filename);
          if((fp = fopen(filename, "wb")) == 0)
          printf("File Open Error. Aborting\n");
        else
          ReceiveIt(fp);
          }
        else
          printf("Aborting Upload. File conflict\n");
      }
    else
      {
      printf("\nOpening new file\n");
      fp = fopen(filename, "wb");
      if(fp == 0)
          {
          printf("ERROR! Can't open file %s\n", filename);
          }
        else
          ReceiveIt( fp );
      }
      }
  }
/******* strncpy *******/

strncpy (s, t, n)
char *s, *t;int n;
  {
  while( n-- > 0 && (*s++ = *t++) != '\0');
  *s = '\0';
  }



/***************** ReceiveIt *************/
/* Get file using Xmodem ***********/
ReceiveIt( fp )
  int fp;
  {
  register int j, i ;
  int  first, snum,scur, scomp, errors, err, chksum, errmax ;
  int  mode, time, crc, chk1, chk2, ok, transize, cnt ;
  char start;
  char tbuff[1028];

  errmax = errormax ;
  snum = 0 ;
  start = 'C' ;          /* Ask for CRC mode first */
  transize = tranSize ;
  mode = 1 ;             /* initial mode for CRC */
  time = 5 ;             /* initial timeout for CRC mode */
  errors = 0;
  rptr = rcvbuf;
  printf("\nTrying to receive file\n");
/*  ResetCTS();
*/
  do  {
      Send( start ) ;            /* request CRC mode */
      first = Readline( time ) ;        /* look for SOH */
      if(first == -1)
      Send(NAK);
      errors += 1 ;
      }
      while (((first != SOH)&&(first != STX))&&(errors < 4 ));

  start = NAK ;        /* change to Checksum mode and for block NAK */
  if ( errors > 3 )
      {
      time = 5 ;
      printf("\nChanging to Checksum mode\n");
      mode = 0 ;
      Send( start ) ;
      first = Readline( time ) ;
      }

  errors = 0 ;
  do  {
      ok = FALSE ;
      while (!( (first == SOH)||(first == STX)||
        (first == EOT)||(first == timeout)) )
      first = Readline( time  );
      if( first == timeout )
           printf("\nTimeout Error # %d - No starting SOH.\n", errors+1 );
      if( (first == SOH)||(first == STX) )
          {
          if(first == SOH)
              transize = 128;
            else
              transize = 1024;
      /* load buffer */
      if(mode == 0 )   /* checksum */
          cnt = transize + 3;
        else
          cnt = transize + 4;
      err = j = 0;
      do  {           /* Read entire block before processing */
          chk1 = Readline(1);
          if(chk1 == -1 )
              err++;
            else
              tbuff[j++] = chk1;
          /* Add keyboard aboort? */
          } while ( ( j < cnt) && (err < 20 ));

      /* Process buffer */
      scur = tbuff[0] ;  /*Readline(1);         /*  real sector number  */
      scomp = tbuff[1];   /*Readline(1);         /* inverse of above */
      if( (j == cnt) && (scur + scomp) == 0xff)
          {
          if( (scur == (0xff & (snum + 1))) )
              {          /* Good sector count */
              switch( mode )
                  {
                    case 0: chksum = 0;
                      for( j = 0; j < transize; j++)
                          {
                          *(rptr+j) = tbuff[j+2] ;  /*Readline(1);*/
                          chksum = (chksum + *(rptr+j)) & 0xff ;
                          }
                      if ( tbuff[j+2] == chksum )   /* Checksum mode */
                         ok = TRUE ;
                      break;
                  case 1: crc = 0 ;          /* CRC mode */
                      for( j = 0; j < transize; j++)
                          {
                          *(rptr+j) = tbuff[j+2];
                          crc = crc ^ (int)*(rptr+j) << 8 ;
                          for(i = 0; i < 8; i++ )
                              if (crc & 0x8000)
                                  crc = crc << 1 ^ 0x1021 ;
                                else
                                  crc = crc << 1 ;
                          }
                      chk1 = tbuff[j+2] ;
                        /*Readline( 1 ) ;    /* hi CRC byte */
                      chk2 = tbuff[j +3];
                      if ( ((chk1 << 8) + chk2 ) == crc )
                          ok = TRUE ;
                      break ;
                  }

              if ( ok )
                  {
                  snum = snum + 1;
                  rptr += transize ;
                  printf("%cReceived sector %d",0xd,snum );
                  errmax = errors + errormax ;
                   /* Good block- Reset error maximum */
                  if( ((unsigned) rptr - (unsigned) rcvbuf) >= BLOCK)
                       {               /* check for full buffer */
                       write( fp, rcvbuf, BLOCK);
                       rptr = rcvbuf;
                       }
                  }
                else
                  {
                  printf("\nError # %d - Bad Block\n", errors+1 );
                  printf("crc %x Chk1 %x chk2 %x ", crc,chk1, chk2);
                  printf("snum %d rcvptr %d\n",snum, rptr);
                  }
              }
            else
                if (scur == (0xff & snum ))
                    {               /* wait until done */
                    while ( (i = Readline(1)) != timeout) ;
                    printf("\nReceived duplicate sector %d\n", scur);
                    start = ACK ;   /* trick error handler */
                    }
                  else
                    {     /* fatal error */
                    printf("\nError # %d - Synchronization error.\n",
                        errors+1 );
                    start =   EOT ;
                    errors += errormax ;
                    }
          }
        else
          {
          printf("\nError # %d ", errors);
          if( scur+scomp != 0xff)
              printf("Header Error - scur %x + scomp %x = %x\n",
                      scur, scomp, scur+scomp );
          if( err > 0)
              printf("    Rcv Err Cnt %d\n", err);
          if( j != cnt)
              printf(" Bytes Read %d Bytes Expected %d\n", j, cnt );
          }
      }
      if (GetCon() == CAN )     /* keyboard cancel */
          {
          first = 0 ;
          errors += errormax ;
          ok = FALSE ;
          start = EOT ;    /* cancel on the other end */
          Send( start );
          }
      if ( ok )    /* got a good sector and finished all processing so - */
          Send( ACK ) ;       /* send request for next sector */
        else
          {              /* some error occurred! */
          errors++ ;
          while ((first = Readline(1)) != timeout) ;
          Send( start );
          start = NAK ;
          }
      first = Readline( 1 ) ;
      } while( (first != EOT)&&(errors <= errmax));

  if(( first == EOT )&&( errors < errmax ))
      {
      Send( ACK );
      printf("\nTransfer complete") ;
      }
    else
      printf("\nAborting");
  if( rptr > rcvbuf)    /* clear buffer */
      {
      while( (rptr - rcvbuf)%BLKSIZE != 0 )
          *rptr++ = 0;
      write( fp, rcvbuf, (rptr - rcvbuf));
      }
  fclose( fp );

  }


/*********************    SendFile ***************************/
/* Gets file name. May include wildcards for Ymodem
/* opens file pointer and calls SendFX for Xmodem SendFY for Ymodem
*/

SendFile()
  {
  char filename[20];
  int fp, j, k;
  char c ;

  FlushCon();
  printf("\n\nFilename.Ext ? ");

  j = getline(filename, 16);
  if (strlen(filename) > 0)
      if(xyModem == 'X')
      {
      fp = fopen( filename, "rb");
      if( fp == 0)
      printf("\nFile open  error!!    Can't open %s %d\n", filename, fp);
    else
      {
      SendFX( fp, filename );
      }
      }
    else
      SendFY(filename);
  }

/********************* Send FX **********************/
/* Send a file XMODEM
*/

SendFX( fp, filename )
  int fp;
  char filename[20];
  {
  static int j, i ;
  static int first, snum, errors, chksum, nbytes, transize, flagEOT ;
  static int crc ;
  static int mode ;    /* 0 = Checksum, 1 = CRC, 2 = Ymodem */
  static char start;   /* block start byte. SOH for 128 byte block
              STX for 1024 */
  int saveTransize ;
  int bytesSent ;
  int fpo ;
  char *chptr ;        /* used to copy filename */


  snum = 1 ;
  mode = 0 ;
  transize = tranSize ;    /*  block transmission size */
  errors = 0;
  flagEOT = 0 ;
  rptr = rcvbuf;
      /* get the first block of data */
      /* Setup to send file name ion first block of 128 bytes.
      /* Zero fill block after filename
      */
  if(xyModem == 'Y')   /* Ymodem transfer, add file name */
      {
      printf("Sending filename %s\n", filename);
      nbytes = read( fp, rcvbuf+128, BLOCK );  /* 128 bytes for name block*/
      nbytes += 128;             /* include fill in bytes read */
      rptr = rcvbuf;
      chptr = filename;
      while(*chptr !=0 )         /* load filename */
      *rptr++ = *chptr++;
      while((rptr - rcvbuf) < 128)     /* zero fill first 128 bytes */
      *rptr++ = 0 ;
      transize = 128;
      snum = 0;
      /********* Debug code
      printf("Filename Block\n");
      for( j = 0; j < 128; j++)
      {
      if(*(rcvbuf+j) > 0x1f)
        putchar(*(rcvbuf+j));
      else
        printf("%d ", *(rcvbuf+j));
      }
       ****  */
      rptr = rcvbuf;     /* Reset rcvbuf pointer to start */
      }
    else
      {
      printf("Reading %d bytes\n", BLOCK);
      nbytes = read( fp, rptr, BLOCK);
      }

printf("Read Data\n");
  if ( nbytes < BLOCK )
      {
      flagEOT += 1 ;       /* flag last block read */
      j = (nbytes/128)*128;
      if(j != nbytes)
      {
      printf("Filling Block with 0x1A\n");
      for(i = nbytes; i < BLOCK + 128; i++)
          *(rptr + i) = SUB;
      }
      }
  if (nbytes == 0 )
      {
      printf( "Error! Empty File\n") ;
      errors = errormax;
      goto ESCAPE ;
      }

  printf("\nTrying to send file %s\ntransize %d Block # %d\n",
      filename, transize, snum);

  errors = 0;
  do{           /* check for initial NAK or C to start transfer */
    first = Readline(1);
    if (first == 'C' )
    {
    mode = 1 ;
    printf( "CRC requested\n" );
    }
      else
    errors ++ ;           /* increment error counter */
    if (GetCon() == CAN)
    {
    printf("Keyboard cancel\n");
    errors = errormax;
    goto ESCAPE;
    }
    } while ( ((first != 'C')&&(first != NAK))&&(errors <  60 )) ;
  if(errors >= 60)     /* 60 second timeout */
      printf( "Error! 60 Second Timeout.  Recieved %xH\n", first );

  if ( (first == 'C')||(first == NAK) )       /* NAK assumes checksum mode */
      {
      errors  = 0 ;       /* reset counter */
      do  {           /* block send loop */
      if(transize > 128)
          start = STX;
        else
          start = SOH;

      printf("%cSending sector # %d Header %x", CR, snum, start );
      if (GetCon() == CAN)
          {
          errors += errormax ;    /* Keyboard abort */
          break;
          }
      Send( start );
      Send( snum ) ;
      Send( ~snum ) ;   /* 1's complement */
      chksum = 0 ;
      crc = 0 ;

      for ( j = 0; j < transize ; j++ )   /* send one block */
          {
          Send( *(rptr+j) );
          switch( mode )
          {
          case 0: chksum += *(rptr+j) ;
              break ;
          case 1: crc = crc ^ (int) *(rptr+j)<<8 ;
              for(i = 0; i < 8; i++ )
                  if (crc &0x8000)
                  crc = crc << 1 ^ 0x1021 ;
                else
                  crc = crc << 1 ;
              break ;
          }
          }
      switch ( mode )
          {
          case 0: Send( chksum ) ;
              break ;
          case 1: Send( (crc >> 8)&0x00ff ) ;
              Send( crc & 0x00ff )    ;
              break ;
          }
      do          /* check for ACK timeout for block transfer */
          {
          first = Readline(1);
          if (first == timeout )
          errors += 1 ;        /* increment error counter */
          }
          while ( (first == timeout)&&(errors < errormax) ) ;
      if ( (first == ACK) ||(first == 'C') ) /* Successful block send,
                           get ready for next block */
          {
          if(snum == 0)       /* sent filename, looking for C */
          {
          if(first != 'C')
              {
              j = Readline(5);
              if(j != 'C')
              printf("\nLooking for C, got %c\n",j);
              if(j == timeout)
              {
              errors = errormax;
              goto ESCAPE;
              }
              }
          rptr += 128 ;    /* update buffer pointer */
          transize = tranSize;
          }
        else
          rptr += transize ;    /* update buffer pointer */
          snum += 1 ;    /* increment sector counter */

           /* Check if file send is complete, get more data if needed*/
          if ( ((unsigned) rptr - (unsigned) rcvbuf) >= nbytes )
          if ( flagEOT > 0 )         /* already read last block */
              nbytes = 0 ;         /* flag done sending */
            else
              {              /* need to fill buffer */
              nbytes = read( fp, rcvbuf, BLOCK );
              printf("\nReading %d bytes!\n", nbytes);
              rptr = rcvbuf ;
              if ( nbytes < BLOCK )
              {
              flagEOT += 1 ;       /* flag last block read */
              j = (nbytes/transize)*transize;
              if(j != nbytes)     /* fill with SUB if needed*/
                  {
                  for(j = nbytes; j < BLOCK; j++)
                  *(rptr + j) = SUB;
                  }
              }

              }
          bytesSent = (unsigned) rptr - (unsigned) rcvbuf;
          if( nbytes - bytesSent < 1024)
          {
          if(transize > 128)
              printf("\nSwitched block size to 128 bytes\n");
          transize = 128;    /* switch to 128 byte blocks */
          }
        else
          transize = tranSize;

          }
        else     /* Error, garbled ACK */
          {
          errors += 1 ;
          switch(first)
          {
          case(NAK): printf("\nNAK Error! Bad Sector # %d\n", snum ) ;
                 break;
          case(CAN): printf("\nSender Cancel\n");
                 errors = errormax;
                 goto ESCAPE;
                 break;
          default:   printf("\nError! Garbled ACK, Sector # ");
                 printf("%d, Got %xH\n", snum, first);
          }
          }
      } while ( (nbytes > 0)&&(errors < errormax ) ) ;
      }

  Send( EOT ) ;
  if ( (first = Readline(10)) != ACK )     /* One freebie NAK */
     {
     Send( EOT) ;
     do {
     if ( (first = Readline(10)) != ACK )
         {
         errors += 1 ;
         printf("\nError! EOT not ACKed. Recieved %xh\n", first );
         Send( EOT ) ;              /* resend on error */
         }
     } while ( (first != ACK)&&( errors < errormax )) ;
     }
  ESCAPE:
  if (errors < errormax )
      printf("\nTransfer complete\n") ;
    else
      printf("\nAborting\n");
  fclose( fp ) ;
  }




/********************* HDOS Specific Routines *********************/

#ifdef HDOS
/******************* set_cline ********************/
set_cline()
{
#asm
    SCALL    MACRO    TYPE
        DB    377Q,TYPE
    ENDM
    mvi    A,0
    lxi    B,03ffh          ; line, echo, no word wrap
    SCALL    6            ; .CONSL
#endasm
}


/******************* set_con_char ********************/
set_cchar()
{
#asm
    mvi    A,0            ; I.CSLMD - console mode
    lxi    B,83ffh         ; Char mode and Word Wrap at width
    SCALL    6            ; .CONSL
    mvi    a,3            ; I.CONWI - console width
    lxi    b,80ffh         ;   set to 80
    scall    6
#endasm
}
#endif
