/* Ymodem related files
/* Autor Darrell Pelan
/* Written for Software Toolworks C80
/* Created 5 Feb 2023
/*
/* make sure option is set to not extend sign bit
*/
#include "printf.h"
#include "mdm.h"

char fcb[36];
char dma[128];
static int treeCnt = 0;
static int Debug = 0;

struct list {
    struct list *next;
    char val[14];
    } *root = 0;


/*************** Get Files to Send ******************
/* unique name or wildcard "*.c" "d?.c"
/* Call Xmodem programs with each file name
*/
SendFY(fn)
   char fn [20] ;
   {
   char ch;

   treeCnt = 0;
   if(DEBUG)
       printf("\nSendFY()\n");
   root = 0;
   GetFiles( fn);
   if(root != 0)
       {
       printf("Send the following files?\n");

       ListFiles(root);
       printf("Ok (Y)? ");
       /*FlushCon();  /*while(getc() != -1);           /* Flush input buffer */
       do ch = toupper(GetCon());
           while( ch == 0);
       printf("%c\n", ch);
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
    int j,k, len, filesSent, first;
    int fp;

    lptr = root;
    filesSent = 0;
    while( lptr != 0)
        {
                         /* get file name, remove spaces */
        len = strlen( lptr->val)+1;
        k = 0;
        for(j = 0; j < len; j++)
            {
            if(*(lptr->val+j) != ' ')
                str[k++] = *(lptr->val+j);
            if(j == 7)
                str[k++] = '.';
            }
/*        printf("Send Batch: %s\n",str);
        /**/
        fp = fopen( str, "rb");             /* open file */
        if( fp == 0)                 /* if file exists Send 128 byte block  */
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
/*  SetCTS();
*/
  loop = 0;
  do  {
      Send('C');
      do  {
          chk1 = Readline(5);
          if(chk1 == -1)
              errors ++;
            else
              buff[cnt++] = chk1;
/*          if(GetCon() == 0x18)
              {
              printf("Keyboard Abort\n");
              return fp;
              }
*/
          } while ((cnt < 133) && (errors < 10));
/*      ResetCTS();      /* stop incomming data */

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
              chk1 = buff[131]&0xff ;     /* hi CRC byte */
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
                  if( j < 17 )       /* file name ok */
                      {
                      sptr = &buff[3];
/*                      ResetCTS();
*/
                      printf("Receiving: %s\n", sptr);
                      fp = fopen(sptr, "ub");
                      if(fp > 0)
                          {
                          printf("Overwrite %s (Y) ", sptr);
                          if(toupper(getchar()) == 'Y')
                              {
                              printf("Overwriting...\n");
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
/*  ResetCTS();
*/
  return fp;
  }

/******* Filename expand programs **********


/************* Get Files  *********************
/* Get files matching wildcard specification
/* input: file name string. may include wildcards "c*.c"
/* output: adds filenames to tree list with root as head
*/
int GetFiles(c)
    char c[20];
    {
    int result, j, k, i, cnt;
    static int max = 20;
    char check;
    int func ;

#ifdef HDOS
	AddList(c);
	return 0;
#else	
    check = 'Y';
    cnt = 1;
    bdos(26,dma);
    expand(c);
    printf("Looking for: %s\n", c);
    makfcb(c,fcb);
    func = 0x11;
    result = 0;
    while(result != -1 && cnt < max)
        {
        result = bdos(func,fcb);
        if(result == -1 )
            {
            if(cnt == 1)
                {
                printf("Find file error\n");
                return -1;
                }
            }
          else
            {
            j = result*32+1;
            for(i = 0; i <11; i++){
                c[i]=dma[j++];
                }
             c[i] = 0;
             AddList(c);
             cnt++;
             if(func == 0x11)
                func = 0x12;
            }
        }
    if(cnt > max)
        printf("\nMore than %d Files found\n", cnt);
    return 0;
#endif	
    }


/************** expand *****************/
/* expands filename wildcard
   returns value in c
*/
expand(c) char c[20];
    {
    int j, k, l, cstop, cptr, rptr;
    char r[20];
    cptr = rptr = 0;

    if(c[1] == ':')    /* check if disk letter */
        cstop = 10;
      else
        cstop = 8;
    while(c[cptr] != 0 && cptr < cstop && c[cptr] != '.')
        {
        if(c[cptr] == '*')
            {
            for(; rptr < cstop; rptr++)
                r[rptr] = '?';
            cptr++;
            }
          else
            r[rptr++] = c[cptr++];
        }
    if(c[cptr] == '.')
        r[rptr++] = c[cptr++];
    k = cptr + 3;
    cstop = rptr +3;
    while(c[cptr] != 0 && cptr < k)
        {
        if(c[cptr] == '*')
            {
            for( ; rptr < cstop; rptr++)
                r[rptr] = '?';
            cptr = cstop;
            }
          else
            r[rptr++] = c[cptr++];
        }
     r[rptr++] = 0;
     for(l = 0; l < rptr; l++ )
         c[l] = r[l];

    }

/************ AddList *****************/
/* add string to list
/* input: string
/* returns void
/* Always starts with global root
*/

AddList( strptr)
    char *strptr;
    {
    struct list *p1, *p2, *new;
    int len, ok;

         /* Allocate memory for new value */
    new = alloc(sizeof(struct list));
    len = strlen(strptr);
    if( len < 14)
        strcpy(new->val, strptr);
      else
        strcpy(new->val, "Name Error");
        /* place new node in list */
    if( root == 0)
        {
        root = new;
        root->next = 0;
        return;
        }
    p1 = p2 = root ;
    ok = FALSE;
    do
        {
        switch(strcmp(strptr, p2->val))
            {
            case 0:  printf("AddList Name equal error\n");         /* equal */
                     ok = TRUE;
                     break;
            case -1: new->next = p2;                       /* less than */
                     if(p2 == root)
                         root = new;
                       else
                         p1->next = new;
                     ok = TRUE;
                     break;
            case 1:  if(p2 != root)                /* greater than */
                         p1 = p2 ;
                     p2 = p2->next;
                     if( p2 == 0)            /* end of list, add node */
                         {
                         p1->next = alloc(sizeof(struct list));
                         p2 = p1->next;
                         strcpy(p2->val, strptr);
                         p2->next = 0;
                         ok = TRUE;
                         }
            }
        } while(!ok);
    }

/************ ListFiles ************/
/* returns the next file on the list
/* input: list pointer
*/

ListFiles(r)
    struct list *r;
    {
    struct list *p;

    p = r;
    while(p != 0)
        {
        printf("%s\n",p->val);
        p = p->next;
        }
    }

/************ FreeList *************/
/* frees memory used by list
/* input: list pointer
/* returns void
*/

FreeList( )
    {
    struct list *p, *r;

    r = root;
    while(r != 0)
        {
        p = r;
        r = r->next;
        free(p);
        }
    root = 0;
    }

