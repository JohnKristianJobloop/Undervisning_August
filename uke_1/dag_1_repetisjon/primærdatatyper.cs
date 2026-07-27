//Primærdatatyper er ting som ints, floats, strings osv osv.


int helt = 10;
int annetHeltTall = 849;

//bit representer staten til en transistor 1 - 0
//en byte er en gruppering av n antal bits, ofte bestemt av operativsystemet, eller programmet du jobber med. 
//som regel er en byte 8 bits. 
//Disse blir gruppert sammen for å representere den "letteste" antall bits processoren kan jobbe med om gangen. 
//I moderne maskiner 32 - 64 bits. 

//For å gjøre dette mer menneskelig forståelig, oversetter vi disse grupperingene av bits, til noe mer forståelig for oss. 
//Da bruker vi forskjellige regler for å få disse 1 og 0 til å representere ting som hele tall, desimal tall, bokstaver m.m.

float desimaltall = 4.5f;

var defaultDecimalDatatype = 56.98;

var superAccurateDecimalValue = 589.934m;


char character = 'C';


//bits -> bytes -> tall

//string

var name = "John";

//Strings er litt spesielt, siden vi kan ha vilkårlig mange forskjellige bokstaver inn i strengen. 

//en string representerer egentlig bare en samling av karakterer av vilkårlig størrelse. 
//Den er readonly og const. 


//Linjen under vil krasje hvis du ukommenterer den:
//name[2] = 'H';


name = name + "!";
Console.WriteLine(name);
