//UnreachableException bor her. Den brukes helt nederst i Match.
using System.Diagnostics;

namespace Result.Core;


//En Result type prøver å løse følgende problem i C# sin Objekt Orienterte kode:
//En metode som kan feile har egentlig TO mulige utfall, men signaturen
//"double Divide(int a, int b)" lyver og lover bare ett. Feilen må da smugles ut
//en annen vei: via exceptions, via null, eller via en out-parameter.
//Felles for alle tre er at den som kaller metoden ikke blir *tvunget* til å bry seg.
//Man kan glemme try/catch, glemme null-sjekken, og koden kompilerer helt fint.
//
//Result<T> flytter feilen inn i selve typen. Returnerer du Result<double>,
//sier signaturen høyt: "her kommer enten en double, eller en feilmelding".
//Og som vi skal se i Match: det finnes ingen måte å få tak i verdien på
//uten samtidig å ta stilling til feilen.

//"abstract" betyr at ingen kan skrive "new Result<int>()", typen finnes bare
//gjennom arvingene sine. "record" gir oss value comparison gratis:
//to Result-er med samme innhold regnes som like (mer om det i Program.cs).
public abstract record Result<T>
{
    //Ved å gjøre standard constructoren privat, prøver vi så godt vi kan å lukke
    //hierakiet. Vi tvinger consumere av apiet å bruke de to public factory metodene Ok og Failure i steden. 
    //Håpet vårt er å enforce så mye som mulig at et objekt av typen Result<T> kun eksisteren som enten en Success type, eller som en Error type. 
    private Result(){}

    //De to tilstandene. Legg merke til at de er "private" og "sealed":
    //  private -> ingen utenfor Result<T> vet i det hele tatt at de eksisterer
    //  sealed  -> ingen kan arve videre fra dem
    //Ingen andre kan derfor skrive "if (x is Success)" og plukke koden vår fra hverandre.
    //De kan kun snakke med Result<T> gjennom metodene vi eksponerer.
    private sealed record Success(T Value) : Result<T>;
    private sealed record Error(string Message) : Result<T>;


    //Vi kan se for oss at vi lager et sett med dører inn og ut av objektet,
    //for å få tilgang til dataen inni. 

    //Her lager vi dørene inn i objektet. Vi må ha to dører, en for hver mulig state.
    //Ok krever en verdi av typen T, Failure krever bare tekst.
    //En feilet Result *har* ingen data å hente ut.
    public static Result<T> Ok(T value) => new Success(value);

    public static Result<T> Failure(string message) => new Error(message);

    //Her lager vi døren for å hente dataen ut igjen.
    //For å komme til verdien må du levere en funksjon for hver mulig state Result kan ha.
    //Du kan altså ikke "bare hente ut verdien og håpe det går bra", slik du kan med
    //en nullable eller en exception du lot være å catche. Compileren tvinger deg
    //til å beskrive hva som skal skje når det gikk galt.
    //
    //Begge grenene må returnere samme type, TResult.
    //TResult kan være hva som helst: en string, et objekt, en ny Result, eller ingenting.
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError)
    {
        //Pattern matching på hvilken av de to tilstandene vi faktisk er.
        //"Success success =>" gjør to ting samtidig: sjekker typen OG gir oss
        //en ferdig castet variabel å bruke på høyresiden.
        return this switch
        {
            Success success => onSuccess(success.Value),
            Error err => onError(err.Message),

            //Denne grenen skal per definisjon aldri kunne kjøre, men compileren
            //vet ikke det og krever at switchen dekker alle tilfeller.
            //UnreachableException er dokumentasjon: treffer du denne, er ikke koden
            //"litt feil", da er en antagelse i designet brutt.
            //(Se notatene nederst i Program.cs, det finnes faktisk ett smutthull.)
            _ =>throw new UnreachableException("There should only be two possible states of a result")
        };
    }

    //Av og til kan det være nyttig å jobbe videre med resultatet før vi
    //pakker ut verdien.
    //Nedenfor er to måter å gjøre dette på. 

    //Map lar deg jobbe videre på verdien uten å pakke ut Result-en.
    //Tenk på det som Select på en liste: du sier hva som skal skje med innholdet,
    //og strukturen rundt består.
    //Er vi i Error-tilstand blir "transform" aldri kalt, feilen bare renner igjennom.
    //Det er derfor vi kan kjede sammen ti operasjoner og likevel bare håndtere feil én gang.
    //
    //Legg merke til at vi bruker Match til å implementere Map. Vi trenger ikke røre
    //Success/Error direkte, Match er allerede den ene måten å inspisere tilstanden på.
    //"Result<TNew>.Failure" uten parenteser er en method group: vi sender selve metoden
    //som funksjon, siden den allerede har akkurat formen Func<string, Result<TNew>>.
    public Result<TNew> Map<TNew>(Func<T, TNew> transform) =>
        Match(value => Result<TNew>.Ok(transform(value)), Result<TNew>.Failure);

    //Bind er Map for operasjoner som selv kan feile.
    //Forskjellen ligger i signaturen til funksjonen du sender inn:
    //  Map  tar Func<T, TNew>          -> "dette går alltid bra"
    //  Bind tar Func<T, Result<TNew>>  -> "dette kan gå galt"
    //Hadde vi brukt Map med en funksjon som returnerer Result, ville vi endt opp med
    //Result<Result<TNew>>, altså en feil pakket inne i en feil. Bind flater det ut for oss.
    //Dette er det som gjør at man kan kjede Divide -> Sqrt -> Parse på rad,
    //der hvert steg kan feile, uten en eneste if.
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> next) =>
        Match(next, Result<TNew>.Failure);
}

//Hvorfor denne? Det handler om type inference.
//C# kan gjette generiske typer ut fra argumentene til en metode, men ikke ut fra
//hvilken klasse du skriver foran punktumet. Derfor må du si det to ganger her:
//      Result<string>.Ok("hei")
//Med en generisk *metode* kan kompilatoren lese typen rett ut av argumentet:
//      ResultFactory.Ok("hei")     -> T blir string helt av seg selv
//
//Failure får vi derimot ikke gratis, den har ingen T å gjette ut fra
//(en feilmelding er bare en string uansett hva suksesstypen skulle vært).
//Der må vi fortsatt skrive ResultFactory.Failure<string>("...") eksplisitt.
public static class ResultFactory
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Failure<T>(string message) => Result<T>.Failure(message);
}
