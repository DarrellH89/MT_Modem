/**************************************************
/* filelib.c File routines for finding files and deletign files
/*Author: Darrell Pelan
/*written for Software Toolworks C80
/*Created 17 Apr 2023 - Added File Delete and Directory functions
/*Last Modified: 23 Apr 2023
**
** Includes versions for CP/M and HDOS. Comment out #ifndef HDOS for CP/M
**
**
** unlink( fname )    delete filename. Adds period to file name if needed
              Terminates string with 80h for HDOS
** scDelete(fn)     Assembly languague delete. Requires period in
              filename to work, string terminanate with 80h
** GetFiles(c )     Get a list of files (c). Supports wildcards in filename
**              uses list struct. Alphabetical file list in root
** expand(c)        Expands wildcards *, ? to ? as needed
** AddList( strptr )    Adds filename to liked list starting with root
              Files are added alphabetically
** FreeList()        Frees memory used by root
** ListFiles()        Prints file list to screen
***
****  NOTE! uses global root defined below
**************************************************/
#include "printf.h"
#include "mdm.h"

#define bufSize 512

#ifdef HDOS
#include "hdosdef.h"
#endif

struct list *root =0;

#ifdef HDOS
/*********unlink(Filename)   *****/
int unlink(fname)
char *fname;
{
    int colon, period, len, fnptr;
    char fn[20];

    /* printf("unlink got: %s\n", fname);*/
    strcpy(fn, fname);
    period = index(fn, ".");
    len = strlen(fname);
    fnptr = fn;
    if (period == -1)
    /* printf("Found period. len: %d\n", len); */
    {
    fn[len] = '.';
    fn[len + 1] = '\0';
    }

    fn[len + 2] = 0x80;
    printf("Deleting %s\n", fn);
    return scDelete(fn);
}

/********Scall Delete *****/

scDelete(fn)
char *fn;
{
#asm
    .z80
    scall macro type
    db 377Q, type
    endm

    pop bc
    pop hl
    push hl
    push bc
;   ld (fn1), hl;
;   ld hl, (fn1)
    ld de, defalt
    scall DELETE
    ld hl, 1
    jp c, done
    ld l, 0
    jp done
defalt: db 'SY0XXX'
fn1: dw 0
done:

    .8080
#endasm
}

/*************Get Files  *********************
/*Get files matching wildcard specification
/*input: file name string. may include wildcards "c*.c"
/*output: adds filenames to tree list with root as head

*/

int GetFiles(c)
char c[20];
{
    int result, j, k, i, cnt, fpdir, bufPtr, drive, bcnt, fcnt;
    static int max = 20;
    char check, buf[bufSize];
    int func, bufcnt;
    static char direct[20] = "SY0:DIRECT.SYS\0";
    char fn[15], found[20];

    expand(c);
    j = strlen(c);
    for (i = 0; i < j; i++)
    c[i] = toupper(c[i]);
    printf("Looking for: %s in %s\n", c, direct);
    drive = index(c, ':');
    if (drive >= 0)
    {
    for (i = 0; i < 3; i++) /*get the drive */
        direct[i] = c[i];
    }
    else
    drive = 0;
    for (i = 0; i < 12; i++) /*setup compare string */
    {
    fn[i] = c[i + drive];
    if (fn[i] == 0)
        break;
    }

    bufcnt = 0;
    fpdir = fopen(direct, "rb");
    if (fpdir > 0)
    {
    cnt = 0;
    while ((result = read(fpdir, buf, bufSize)) == bufSize)
    {
        bufPtr = 0;
        while (bufPtr < bufSize - 6)
        {      /*        printf("bufPtr %d\n",bufPtr);    */
        if (buf[bufPtr] < 0xfe) /*valid entry */
        {
            fcnt = 0;
            bcnt = 0;
            result = 1; /*assume success */
            while (bcnt < 11 && result == 1)
            {
            if (fn[fcnt] == '.')
                {
                while (bcnt < 8 && result == 1)
                if (buf[bufPtr + bcnt++] != 0)
                    result = 0;
                fcnt++;
                }
            if ((fn[fcnt] == buf[bufPtr + bcnt]) ||
                (fn[fcnt] == '?' ) ||
                (fn[fcnt] == 0 && buf[bufPtr + bcnt] == 0))
            {
                if( fn[fcnt] >0)
                fcnt++;
                bcnt++;
            }
            else
                result = 0;
            }
            if (bcnt == 11) /*found a match */
            {
            if (drive > 0)
                for (i = 0; i < 4; i++)
                found[i] = c[i];
            k = drive;
            for (i = 0; i < 11; i++)
                switch (buf[bufPtr + i])
                {
                case 0:
                    break;
                default:
                    if (i == 8)
                    found[k++] = '.';
                    found[k++] = buf[bufPtr + i];
                }

            found[k] = 0;
            AddList(found);
            cnt++;
            }
        }
        bufPtr += DIRLEN;
        }
    }

    }

    fclose(fpdir);
    if (cnt > max)
    printf("\nMore than %d Files found\n", cnt);
    return 0;
}

/************** expand *****************/
/* expands filename wildcard
   returns value in c
*/
expand(c)
char c[20];
{
    int j, k, l, cstop, cptr, rptr;
    char r[20];
    cptr = rptr = 0;

    if (c[3] == ':') /*check if disk specified */
    cstop = 12;
    else
    cstop = 8;
    while (c[cptr] != 0 && cptr < cstop && c[cptr] != '.')
    {
    if (c[cptr] == '*')
    {
        for (; rptr < cstop; rptr++)
        r[rptr] = '?';
        cptr++;
    }
    else
        r[rptr++] = c[cptr++];
    }

    if (c[cptr] == '.') /*check for period */
    r[rptr++] = c[cptr++];
    else
    r[rptr++] = '.';
    k = cptr + 3;
    cstop = rptr + 3;
    /* handle file extension     */
    while(c[cptr] != 0 && cptr < k)
    {
    if(c[cptr] == '*')
        {
        for(; rptr < cstop; rptr++)
        r[rptr] = '?';
        cptr = cstop;
        }
      else
        r[rptr++] = c[cptr++];
    }
    r[rptr++] = 0;
    for(l = 0; l < rptr; l++)      /* copy result into input string */
    c[l] = r[l];
}
#endif

/******* Filename expand programs **********/

#ifndef HDOS

char fcb[36];
char dma[128];


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
#endif

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
    if(  root == 0)
    {
    root = new;
    root->next = 0;
    return;
    }
    p1 = p2 =  root ;
    ok = FALSE;
    do
    {
    switch(strcmp(strptr, p2->val))
        {
        case 0:  printf("AddList Name equal error\n");       /* equal */
             ok = TRUE;
             break;
        case -1: new->next = p2;               /* less than */
             if(p2 ==  root)
             root = new;
               else
             p1->next = new;
             ok = TRUE;
             break;
        case 1:  if(p2 != root)           /* greater than */
             p1 = p2 ;
             p2 = p2->next;
             if( p2 == 0)         /* end of list, add node */
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

ListFiles( )
    {
    struct list *p;

    p = root;
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
