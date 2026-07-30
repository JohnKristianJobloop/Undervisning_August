using Result.Core;


//Legg merke til returtypen. Siden Result<string> sin signatur er enten "Ok eller Fail"
//Kan vi se for oss signaturen til Result som en forklaring av hva som vil skje:
//"du får en string med resultatet tilbake, ELLER en forklaring på hvorfor du ikke fikk det".
//Sammenlign med en vanlig "string DivideTwoNumbers(int, int)", som ville lovet
//en string uansett, og så kastet en exception ut fra intet på den som kaller.
Result<string> DivideTwoNumbers(int num1, int num2)
{
    //Feiltilfellet håndteres først, og vi kommer oss ut med en gang.
    //Ingen exception kastes, feilen er bare en helt vanlig returverdi.
    if (num2 == 0)
    {
        return ResultFactory.Failure<string>("Cannot divide by zero");
    }

    return ResultFactory.Ok($"Result is: {(double)num1 / num2}");
}

//Samme funksjon, men den returnerer selve tallet i stedet for en ferdig setning.
//Dette er den mest nyttige varianten: så lenge vi holder på en double kan vi
//regne videre på den (se Map lenger ned). Formaterer vi til string med en gang,
//har vi kastet fra oss muligheten.
Result<double> Divide(int num1, int num2)
{
    if (num2 == 0)
    {
        return ResultFactory.Failure<double>("Cannot divide by zero");
    }
    return ResultFactory.Ok((double)num1 / num2);
}

//Liten tangent om hvorfor Result er en record og ikke en class.
//To lister med nøyaktig samme innhold:
List<int> numbers = [1,2,3];
List<int> numbers2 = [1,2,3];

//Selv om de to listene har samme verdi, er de to forskjellige objekter. 
//List<T> er en vanlig klasse, og == på klasser sammenligner referanser:
//"er dette det samme objektet i minnet?" Sammenligningen skipper verdiene helt.
Console.WriteLine(numbers == numbers2);




//La oss se på to verdier og sammenligne.
//To feilede Result-er med samme melding:
var result = DivideTwoNumbers(10,0);
var result2 = DivideTwoNumbers(10,0);

//Records genererer value comparison automatisk: samme type og samme innhold
//i alle feltene betyr like. To feil med samme melding ER den samme feilen,
//og det er som regel akkurat det vi ønsker når vi tester og sammenligner.
Console.WriteLine(result == result2);


//Slik pakker vi ut en Result. 
//Vi ber ikke om verdien, vi leverer inn
//en oppskrift for hvert utfall og får tilbake det ene svaret som gjaldt.
//Her returnerer både good path og bad path en string, så resultMessage blir en string.
//Det finnes ingen vei rundt. Vi *må* skrive err-grenen for å komme
//til success-grenen. Denne måten å skrive kode på tvinger "consumeren" av Result Apiet vårt
//å tenke på error håndtering.

//Programmeringspråk som Go har ofte denne filosofien bakt inn i standardbiblioteket til språket. 
//Go, i motsetning til C#, leverer alltid tilbake et resultat fra hvert funksjonskall. 
//Det er mange "if err =! nil" memes som stammer fra go, hvor man etter hver operasjon må skjekke dette:

// let res, err = DoSomething(42)
// if err != nil{
//   printf("Something went very wrong!");
//}
//
// C# har ikke samme støtte i språket. 
// Vi oppnår det samme med Try / Catch, men noen programmerere liker ikke
// at kode kan throwe exceptions (som ikke bare er en interrupt, men også en heeeeelt annen returntype / objekttype enn forventet)
// og ønsker ofte å kode på denne måten. 
var resultMessage = result.Match(
    success => "Success! " + success,
    err => "Failure! " + err
    );

Console.WriteLine(resultMessage);

//Før vi fikset accessors ville følgende kode crashe programmet vårt:
//Result<string> rogue = new Rogue<string>();
//var res = rogue.Match(s => s, e => e);
//record Rogue<T> : Result<T>;

//Vi ordnet dette ved å kommunisere bedre i record definisjonen vår
//ved å gjøre Result<T> abstrakt, og default constructoren privat.

//Desverre ønsket vi å bruke value comparison til records, og records har en
//annen, skjult constructor. Den kan vi ikke gjøre noe med, så koden nedenfor
//Vil likevell krashe koden vår:

//Selv med alle accessor triksene våre vil koden nedenfor fremdeles krasje programmet vårt.
//Andre kan likevell få tilgang til uønskede deler av vårt api:

// var rogue = new Rouge<string>(ResultFactory.Ok("Hello"!)).Match(success => success, err => err);
// record Rogue<T>(Result<T> other) : Result<T>(other)

//En Rogue er verken Success eller Error, og da faller Match ned i "_"-grenen
//og kaster UnreachableException. Det er nettopp derfor den grenen finnes.
//Lærdommen: C# har ikke ekte discriminated unions, så vi kommer langt med
//accessors, men helt vanntett blir det ikke.

//Litt om Map til Result. 
//Select er LINQ sin Map. Nøyaktig samme idé som Result.Map:
//vi sier hva som skal skje med hvert element, og strukturen (her: sekvensen)
//består. Result er bare en "container" som rommer maks 1 verdi.
var strings = numbers.Select(num => num.ToString());


//Her ser vi hvorfor Divide returnerer double og ikke string.
var divisionResult = Divide(4,5);

//Map kjører ganger-to *inni* Result-en. Går det bra, blir 0.8 til 1.6.
//Hadde vi delt på null, ville transform-funksjonen aldri blitt kalt,
//og feilmeldingen bare rent rett igjennom til Match under.
//
//Merk at Match-grenene her ikke er symmetriske: den ene returnerer en double,
//den andre kaster. Det er lov, fordi throw ikke produserer en verdi i det hele tatt.
//Da har vi riktignok kastet bort halve poenget med Result, men det er et bevisst
//valg her og nå: vi sier "på dette punktet i programmet er en feil uopprettelig".
//Forskjellen fra en vanlig exception er at valget er synlig i koden.
var multiplied = divisionResult.Map(num => num * 2).Match(success => success, err => throw new ArgumentException(err));

Console.WriteLine(multiplied);
