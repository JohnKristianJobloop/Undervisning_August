using System.Net;
using System.Net.Sockets;
using System.Text;

//En webserver er ikke magi. Den er en socket som lytter på en port,
//leser tekst fra en TCP-forbindelse, og skriver tekst tilbake.
//Alt ASP.NET Core gjør, er dette pluss veldig mange lag med bekvemmelighet.
//
//To lag er i sving her, og det er verdt å holde dem fra hverandre:
//      TCP  -> transporten. Sørger for at bytes kommer frem, i riktig rekkefølge.
//              TCP vet ingenting om "GET" eller "200 OK", det er bare en byte-strøm.
//      HTTP -> avtalen om hva bytene BETYR. Ren tekst, med et fast format
//              vi begge har blitt enige om på forhånd.

//TcpListener er "resepsjonisten". Den tar imot innkommende forbindelser på en port.
//IPAddress.Any betyr "lytt på alle nettverkskortene på maskinen", ikke bare localhost.
//Vil du kun slippe til trafikk fra din egen maskin, bruker du IPAddress.Loopback.
var listener = new TcpListener(IPAddress.Any, 8080);

//Frem til Start() er dette bare et objekt. Det er her OSet faktisk reserverer porten
//for oss og begynner å legge innkommende forbindelser i kø.
//Kjører noe annet allerede på 8080, er det her det smeller.
listener.Start();
Console.WriteLine("Listening on localhost:8080");

//Accept-loopen. Denne kjører til vi dreper prosessen, en server er per definisjon
//et program som aldri blir ferdig.
while (true)
{
    //AcceptTcpClient BLOKKERER tråden til noen faktisk kobler seg til.
    //Det høres ille ut, men her er det riktig: denne tråden har uansett
    //ingenting annet å gjøre enn å vente på neste kunde.
    //Hver client vi får er én forbindelse, altså én "samtale".
    var client = listener.AcceptTcpClient();

    //Her er hele poenget med async i en server.
    //Vi kaller HandleClient, men vi AWAITER IKKE. Vi kaster Tasken i en discard (_).
    //Det kalles "fire and forget": vi starter jobben og går umiddelbart tilbake
    //til toppen av loopen for å ta imot neste client.
    //
    //Hadde vi skrevet "await HandleClient(client);" ville serveren håndtert
    //nøyaktig ÉN bruker om gangen. Bruker nummer to måtte stått i kø til
    //bruker nummer én var helt ferdig.
    //
    //_ = er ikke bare pynt: den sier til kompilatoren (og til deg som leser)
    //"jeg vet at jeg ignorerer denne Tasken, det er med vilje".
    //Prisen vi betaler: kaster HandleClient et exception, er det ingen som
    //fanger det opp. I en ekte server ville vi hatt try/catch inne i metoden.
    _= HandleClient(client);
}


//All håndtering av én enkelt forbindelse skjer her.
//Merk at metoden er "static" fordi vi bruker top-level statements,
//den har ingen tilstand den trenger å dele med noen.
static async Task HandleClient(TcpClient client)
{
    //using = "lukk dette når vi er ferdige, uansett om det går galt underveis".
    //For en socket er dette ikke valgfritt: gjør vi det ikke, lekker vi
    //forbindelser til vi går tom, og klienten blir hengende og vente på svar.
    using (client)

    //NetworkStream er selve røret mellom oss og klienten.
    //Det er en helt vanlig Stream, samme API som en FileStream.
    //Vi leser bytes ut av den, og skriver bytes inn i den.
    using (NetworkStream stream = client.GetStream())
    {
        //Vi må ha et sted å legge bytene vi leser. 1024 bytes er en forenkling!
        //TCP er en STRØM, ikke meldinger. Det finnes ingen garanti for at hele
        //requesten kom i én pakke, eller at den får plass i 1024 bytes.
        //En ekte server leser i en loop til den har sett den tomme linjen
        //som markerer slutten på headerne. Her later vi som om alt kommer på én gang.
        byte[] buffer = new byte[1024];

        //Her er det async faktisk tjener oss noe.
        //Å vente på nettverket tar en evighet i CPU-tid. Med await slipper tråden
        //taket mens vi venter, og kan gjøre nytte for seg for en annen client.
        //bytesRead forteller hvor mange av de 1024 plassene som faktisk ble fylt.
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        //Bytes -> tekst. HTTP er ren tekst, så vi kan lese requesten med øynene.
        //Vi dekoder KUN de bytesene vi faktisk fikk (0 til bytesRead),
        //resten av bufferet er bare nuller.
        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        //Skriv den ut i konsollen og se på den. Dette er hele hemmeligheten
        //bak HTTP, det er bare tekst med et avtalt format:
        //      GET / HTTP/1.1
        //      Host: localhost:8080
        //      User-Agent: ...
        //      (tom linje)
        //      (eventuell body)
        Console.WriteLine(request);

        //Linjeskift i HTTP er alltid \r\n (CRLF), ikke bare \n.
        //Første linje kalles "request line", og den er alt vi trenger her.
        string requestLine = request.Split("\r\n")[0];

        //Request line er tre deler skilt med mellomrom: METODE STI VERSJON
        //      "GET /index.html HTTP/1.1"
        //Vi plukker ut metoden. Hadde vi tatt [1] hadde vi fått stien,
        //og da begynner vi å ha en ruter, altså begynnelsen på ASP.NET Core.
        string method = requestLine.Split(" ")[0];

        //Vår "ruting" i sin enkleste form: er det GET, svarer vi pent.
        //Alt annet får 405. Statuskoden er ikke pynt, det er slik klienten
        //vet om den lyktes uten å måtte lese teksten vi sendte.
        string response = method == "GET"
            ? BuildResponse("200 OK", "Hello from my dumb webserver!")
            : BuildResponse("405 Method Not Allowed", "Get the heck away!");

        //Tilbake til bytes, for det er det eneste en socket kan sende.
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

        //Når vi faller ut av blokken her, kaller using Dispose() på både
        //stream og client. Det er DET som faktisk lukker forbindelsen
        //og forteller nettleseren at svaret er ferdig.
    }
}

//Et HTTP-svar har nøyaktig samme form som en request, bare speilvendt:
//      1. statuslinje    -> "HTTP/1.1 200 OK"
//      2. headere        -> metadata om svaret, én per linje
//      3. TOM LINJE      -> helt kritisk, dette er skillet mellom header og body
//      4. body           -> selve innholdet
//Glemmer du den tomme linjen, blir nettleseren bare stående og vente.
//
//Content-Type sier hvordan klienten skal TOLKE bodyen (prøv "text/html" og se!).
//Content-Length sier hvor mange bytes den skal lese, slik at klienten vet
//når svaret er slutt. Merk at body.Length teller TEGN, ikke bytes, det holder
//så lenge alt er ASCII, men putter du inn æ/ø/å blir tallet for lavt.
//Connection: close sier at vi lukker forbindelsen etterpå, i stedet for å
//holde den åpen for flere requests (som er standard i HTTP/1.1).
static string BuildResponse(string status, string body) => $"""
HTTP/1.1 {status}
Content-Type: text/plain
Content-Length: {body.Length}
Connection: close

{body}
""";
