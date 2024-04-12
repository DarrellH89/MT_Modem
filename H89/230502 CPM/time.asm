        XTEXT   HOSDEF
TICCNT  EQU     40033A          Clock Tick counter

        ORG     42200A

ENTRY   LXI     H,0
        SHLD    TICCNT                  Zero the Tick counter

        MVI     A,'!'
        SCALL   .SCOUT
        CALL    DELAY
        MVI     A,'.'
        SCALL   .SCOUT
        CALL    DELAY
        MVI     A,'!'
        SCALL   .SCOUT

        LHLD    TICCNT                  Get the number of TICCNTs
        LXI     B,0FFFFH
        LXI     D,-500                  Number of TICCNTs per second
DIV     INR     B
        DAD     D
        JC      DIV                     Subtract until negative result
        LXI     D,500
        DAD     D                       Resotre remainder to HL
        LXI     D,-50
DIV.1   INR     C
        DAD     D
        JC      DIV.1                   Subtract until negative result
        CALL    TIMEOUT

        XRA     A
        SCALL   .EXIT

TIMEOUT PUSH    BC                      Save the seconds counter
        LXI     H,MESS
        SCALL   .PRINT                  Print the execution time message
        POP     BC                      Get my counter back
        MOV     A,B
        PUSH    BC
        CALL    NUMOUT
        MVI     A,'.'
        SCALL   .SCOUT
        POP     BC
        MOV     A,C
        CALL    NUMOUT
        LXI     H,MESS1
        SCALL   .PRINT
        RET

*
MESS    DB      0AH,0A,9,'Time executed in',' '+80H
MESS1   DB      ' seconds','.'+80H
*
NUMOUT  ANI     0FH                     Program takes less than 10 seconds
        ADI     '0'                      so we don't have to error check
        SCALL   .SCOUT
        RET
*
DELAY   LXI     H,0
        CALL    LOOP
        CALL    LOOP            HL is already set to 0 for next delay
        RET
*
LOOP    DCX     H
        MO