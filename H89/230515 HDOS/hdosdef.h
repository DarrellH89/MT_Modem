/* HDOS DEF
*/

#define EXIT	0	/*EXIT TO OPERATING SYSTEM   */
#define SCIN	1	/*INPUT CHARACTER FROM SYSTEM CONSOLE  */
#define SCOUT	2	/*OUTPUT CHARACTER TO SYSTEM CONSOLE  */
#define PRINT	3	/*PRINT LINE ON SYSTEM CONSOLE	*/
#define CONSL	6	/*SET/CLEAR CONSOLE MODES  */
#define LINK	20h	/* LINK to another program  */
#define SETTOP	2Ah	/* Get available memory  */
#define NAME	2Ch	/* Get open channel name */
#define DELETE	28h	/* Delete file */

#define STACDK	 2280H	 /*STACK GROWS DOWNWARD FROM HERE */
#define USERFWA  2280H	 /*FWA OF USER MEMORY AREA  */
#define DIRLEN	 0x17
