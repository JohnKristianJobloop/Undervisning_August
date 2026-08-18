namespace Core;

//Domenemodellen vår. 
//Dette er en primary constructor (C# 12). Parameterne (title, body) hører til
//HELE klassen, ikke bare til en konstruktørblokk, og kan brukes til å
//initialisere properties under.
//Standard Constructor, som gjør nøyaktig det samme:
/*
* public class TodoItem
* {
*     public TodoItem(string title, string body)
*     {
*         Title = title;
*         Body = body;
*     }
* }
*/
public class TodoItem(string title, string body)
{
    //init betyr "kan settes når objektet lages, aldri etterpå".
    //En Id som kan endres i ettertid er en Id du ikke kan stole på.
    //Guid gir oss dessuten en id vi kan lage lokalt, her og nå, uten å måtte
    //spørre en database om neste ledige nummer slik vi måtte med en int.
    public Guid Id {get;init;} = Guid.NewGuid();

    //Disse to skal kunne endres etter at objektet er laget
    //(se UpdateTitle/UpdateBody i TodoItemExtensions), derfor set og ikke init.
    public string Title {get;set;} = title;
    public string Body {get;set;} = body;

    //Tid er litt sært. Vi har flere tidssoner og flere tidstyper.
    //DateTime.Now refererer til din tid på din maskin i din tidssone. Det kan skape konflikter mellom brukere. 
    //Derfor bruker vi UtcNow, ikke Now. Serveren kan stå i en annen tidssone enn brukeren,
    //og så finnes sommertid.
    //Lagre Tidsverdier i en konstant tidssone, f.eks UtcNow, så konverter til LocaleTime senere når det skal vises.
    public DateTime CreatedAt{get;init;} = DateTime.UtcNow;

    //Her har vi en nullable DateTime. Ved å ta i bruk null som basisverdi her
    //kan vi ganske raskt se om en TodoItem er "ferdig eller ikke".
    public DateTime? CompletedAt{get;set;} = null; 

    //Computed property: dette feltet tar ingen faktisk fysisk plass, den regnes ut på
    //nytt hver gang noen leser den. 
    //=> er ikke en lambda her, det er en
    //expression-bodied member, kortform for { get { return ...; } }.
    //Gevinsten er at det er umulig å havne i en tilstand der IsComplete sier
    //true mens CompletedAt er tom. 
    public bool IsComplete => CompletedAt is not null;
}
