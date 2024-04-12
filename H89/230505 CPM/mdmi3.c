/* Ymodem related files
/* Author Darrell Pelan
/* Written for Software Toolworks C80
/* Created 5 Feb 2023
/*
/* make sure option is set to not extend sign bit
/****** */
#include "printf.h"
#include "mdm.h"

/* buffer comment */
static int Debug = 0;
extern struct list *root;

/*************** Get Files to Send ******************
/* unique name or wildcard "*.c" "d?.c"
/* Call Xmodem programs with each file name
*/
SendFY(fn)
   char fn [20] ;
   {
   char ch;

   if(DEBUG)
       printf("\nSendFY()\n");
   root = 0;
   GetFiles( fn );
   if(root != 0)
       {
       printf("Send the following files?\n");

       ListFiles();
       printf("Ok (Y)? ");
       /*FlushCon();  /*while(getc() != -1);           /* Flush input buffer */
       do ch = toupper(GetCon());
       while (ch == 0);
       if(ch == 'Y')
      {
      printf("\nSending Files \n");
      SendBatch();
      }
    else
      printf("\nBatch Send Cancelled %xH\n", ch);
       FreeList();
       }
   }

/************** SendBatch **********************/
/* Sends files in root list using Ymodem
/* Input: None
/* Returns void
*/
SendBatch()
    {
    struct list *lptr;
    char str[14], ch;
    int j,k, len, filesSent, first, per;
    int fp;

    lptr = root;
    filesSent = 0;
    while( lptr != 0)
    {
             /* get file name, remove spaces */
    len = strlen( lptr->val)+1;
    k = 0;
    per = 0;
    for(j = 0; j < len; j++)
        {
        if(*(lptr->val+j) != ' ')
        str[k++] = *(lptr->val+j);
        if(str[k-1] == '.')
        per = j ;
        if(j == 7 && per == 0)
        str[k++] = '.';
        }
/*      printf("Send Batch: %s\n",str);
    /**/
    fp = fopen( str, "rb");         /* open file */
    if( fp == 0)             /* if file exists Send 128 byte block  */
        printf("\nFile open error! %s\n",str);
      else
        {
        SendFX( fp, str);
        if(DEBUG)
        printf("Return from SendFX\n");
        filesSent++;
        }
             /* use SendFX to send file */
    lptr = lptr->next;
    }
    if(filesSent > 0)
    {
    printf("Sending Ymodem end block\n");
    first = Readline(20);
    if(DEBUG)
         printf("Got %x\n", first);
    if(first == 'C')
        {
        Send(SOH);
        Send(0);
        Send(0xff);
        for(j = 0; j < 130; j++)
        Send(0);
        if((first = Readline(20)) != ACK)
        printf("Ymodem end block not ACKed\n");
        }
    }

    }

/************** GetYmodem **************
/* Receive batch files
/* Get Filename from Serial input
/* return file pointer
*/

int GetYmodem()
  {
  int fp, first, errors, crc, j, cnt, dataOK, i, chk1, chk2, loop;
  char buff[150], c, *sptr;

  errors = 0;
  fp = 0;
  cnt = 0;
  loop = 0;
  do  {
      Send('C');
      do  {
      chk1 = Readline(5);
      if(chk1 == -1)
          errors ++;
        else
          buff[cnt++] = chk1;
        if(GetCon() == 0x18)
          {
          printf("Keyboard Abort\n");
          return fp;
          }
      } while ((cnt < 133) && (errors < 10));
      first = buff[0];
      if(DEBUG)
          {
          printf("Sent C, Got %x, Errors %d \n", first, errors);
          for(j = 0; j < cnt; j++)     /* print the buffer */
              printf("%x ", buff[j]);
          printf("\nLoop %d\n", loop);
          }
      if(first == EOT)
          {
          fp = 0;
          break;
          }
      if (first == SOH)
          {
          chk1 = buff[1];
          chk2 = buff[2];
          if((chk1 + chk2)&0xff == 0xff)
              {
              for(j = 3; j < cnt-2; j++)     /* Data CRC */
                  {
                  crc = crc ^ buff[j] << 8 ;
                  for(i = 0; i < 8; i++ )
                      if (crc & 0x8000)
                          crc = crc << 1 ^ 0x1021 ;
                        else
                          crc = crc << 1 ;
                  }
              chk1 = buff[131]&0xff ;      /* hi CRC byte */
              chk2 = buff[132]&0xff;      /* lo CRC byte */
              if(DEBUG)
                  printf("chk1 %x chk2 %x CRC %x\n", chk1, chk2, crc);
              if ( ((chk1 << 8) + chk2 ) == crc )   /* CRC matches */
                  {
                  j = 3;
                  while(buff[j] != 0 && j < 17)
                      j++;
                  if( j == 3)
                       {
                       Send(ACK);
                       return fp;
                       }
                  if( j < 17 )         /* file name ok */
                      {
                      sptr = &buff[3];
                      printf("Receiving: %s\n",sptr);
                      fp = fopen( sptr, "ub");
                      if( fp > 0 )
                          {
                          printf("Overwrite %s (Y)\n", sptr);
                          if(toupper(getchar()) == 'Y')
                              {
                              printf("\nOverwriting...\n");
                              fclose( fp );
                              unlink( sptr );
                              fp = fopen( sptr, "wb");
                              }
                          }
                        else
                          {
                          printf("Opening new file \n");
                          fp = fopen( sptr, "wb");
                          }
                      if ( fp == 0 )
                          printf("File creation error: %s\n", sptr);
                      }
                    else
                      printf("Filename Error\n");
                  }
                else
                  printf("\nCRC fail\n");
              }
          }
      if(fp == 0)
          {
          do  chk1 = readline(1);
              while (chk1 != -1);
          Send( NAK);
          }
        else
          Send(ACK);
      } while ((loop++ < 4)&& (fp == 0));
  if( fp == 0)
      Send(CAN);     /* Cancel Send */
    else
      Send(ACK);
  return fp;
  }


