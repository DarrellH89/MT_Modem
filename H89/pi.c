/**********
/* Pi Double Precision Bionomial Theorem Version
/* Author: Darrell Pelan
/* 5 April 2023
**********************/

#include "fprintf.h"

/************* pow 10 **********************/
float pow10( num)
int num;
  {
  float rst ;
  int i;

  rst = 1.0;
  if(num >= 0)
      for(i = 0; i < num; i++)
          rst *= 10;
    else
      for( i = 0; i < (0-num); i++)
          rst *= 0.1;
  return rst ;
  }

/************* sqrt **********************/
float sqrt( n)
  float n;
  {
  float z, rst, j;
  int max, i;

  max = 10;
  rst = 0.0;
  z = n;
  j = 1.0 ;

  for(i = max; i > 0; i--)
    {
    if(z-((2*rst)+(j*pow10(i)))*(j*pow10(i)) >= 0)
        {
        while(z-((2*rst)+(j*pow10(i)))*(j*pow10(i))>=0)
            {
            j++;
            if(j >=10)
                break;
            }
        j--;
        z -= ((2*rst)+(j*pow10(i)))*(j*pow10(i));
        rst += j*pow10(i);
        j = 1.0;
        }
    }
  for(i = 0; i >= 0-max; i--)
    if(z-((2*rst)+(j* pow10(i)))*(j*pow10(i)) >= 0)
      {
      while(z-((2*rst)+(j*pow10(i)))*(j*pow10(i))>=0)
        j++;
      j--;
      z -= ((2*rst)+(j*pow10(i)))*(j*pow10(i));
      rst += j*pow10(i);
      j = 1.0;
      }

  return rst;
  }

/************* Print Comma **********************/

PrintComma(num, field)
  long num;
  int field;
{
    int n2[5] ;
    static int cnt = 0;
    int flag ;

    while(field >0)
    {
        n2[cnt++] = (num % 1000);
        num /= 1000;
        field -= 3;
    }

    flag = 0;
    if (n2[--cnt] == 0)
        printf("   ");
    else
    {
        printf("%3d", n2[cnt]);
        flag++;
    }

    while(cnt > 0)
    {
        if (n2[--cnt] == 0 && flag == 0)
            printf("    ");
        else
        {
            if(flag == 0)
                if(n2[cnt] == 0)
                   printf("    ");
                 else
                   {
                   printf("%3d", n2[cnt]);
                   flag++;
                   }
            else
                {
                printf(",%03d", n2[cnt]);
                flag++;
                }
        }
    }
}

/************* main Pi.C **********************/

main()
{
    static int range = 20;
    static int n = 3;
    int     j;
    float sides ;
    long sidel;
    float test;
    float k, sum, ssq, term, temp, fact, slength, pilow, pihigh;

    sides = 4.0;

    printf("    Bounds on Pi - Double Precision Bionomial Theorm Version\n");
    printf("\n N          Sides     Side Length    Pi-Lower Bound  Pi-Upper Bound\n");

    sum = 2.0;
    for (j = 2; j < range; j++)
    {
        sides = sides * 2;
        ssq = sum;
        sum = 0.0;
        term = ssq * 0.25;
        k = 1.0;

        temp = term + sum;
        while (temp > sum)
        {
            sum = temp;
            fact = (2 * k - 1) / (k + 1);
            term = fact * ssq * term / 8;
            k++;
            temp = term + sum;
         }
        slength = sqrt(sum);
        test = 4.0;
        pilow = sides * 0.5 * slength;
        pihigh = sides * slength / (2 - slength);
        printf("%2d ", n);
        sidel = sides;
        PrintComma(sidel,10);
        printf(" %15.12f %15.12f %15.12f\n", slength, pilow, pihigh);
        n++;
    }

}
