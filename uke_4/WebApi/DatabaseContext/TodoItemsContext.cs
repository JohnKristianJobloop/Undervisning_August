namespace WebApi.DatabaseContext;
using Core;
using Microsoft.EntityFrameworkCore;

//Her bytter vi ut hele lagringen vår. Frem til nå har todoene ligget i en
//List<TodoItem> inne i TodoItemRepository, og forsvunnet i det appen stoppet.
//Nå ligger de i en SQLite-database på disk: Data/TodoItemDatabase.db.
//
//Legg merke til hva som IKKE måtte endres: controlleren. Den spør fortsatt
//bare etter ITodoItemRepository. Dette er løftet fra Dependency Inversion som
//blir innfridd, vi bytter ut hele det nederste laget uten å røre det øverste.
//
//DbContext er EF Core sin representasjon av "en økt mot databasen", og gjør
//tre jobber for oss:
//      1. Oversetter LINQ (C#) til SQL, og gjør radene med data som kommer tilbake om til TodoItem-objekter
//      2. Holder oversikt over endringer på objektene den har lest (change tracking)
//      3. Skriver alle endringene ned i én transaksjon når vi kaller SaveChanges()
//
//At denne klassen både ER en DbContext og implementerer ITodoItemRepository er
//en snarvei. Den er grei her fordi det er lite kode, og fordi DbContext i seg
//selv allerede er både et repository- og et unit-of-work-mønster. I et større
//prosjekt ser man like gjerne en egen repository-klasse som TAR IMOT en
//DbContext via konstruktøren, i stedet for å arve fra den.
//
//options kommer inn via Dependency Injection, se
//Extensions/TodoItemsContextServiceCollectionExtension.cs.
//Det er der vi sier "dette er SQLite, og her er connection stringen". 
//Contexten selv vet ikke
//hvilken database den snakker med. Skal vi over på PostgreSQL en dag, endrer
//vi én linje der borte, og ingenting i denne filen.
//
//
//====================================================================
// MODELLEN FØRST: fra C#-klasser til tabeller
//====================================================================
//I dette prosjektet jobber vi kun med SQL via vår kode. EF-Core gir oss muligheten å 
//jobbe med SQL via en syntaks vi allerede skjenner til. 
//Dette prosjektet jobber med en Model-First aproach, som vil si
//vi lager en datamodell for hvordan vi vil databasen vår skal se ut, før vi ber ef-core bygge den for oss.
//(Googler du dette, finner du det oftest under navnet
//"code first/model first". Det motsatte, å scaffolde C#-klasser ut fra en database som
//allerede finnes, heter "database first" og gjøres med
//`dotnet ef dbcontext scaffold`.)
//
//Steg 1: EF Core bygger en MODELL ved å lese denne klassen med refleksjon.
//        Hver DbSet<T> blir en tabell, hver public property på T blir en
//        kolonne, en property som heter Id blir primærnøkkel, DateTime? blir
//        en NULL-bar kolonne mens DateTime blir NOT NULL. Entity Framework Core klarer
//        disse oversettelsene pga konvensjoner. 
//           - property som heter Id? Det blir databasen sin primary key. 
//           - er det et objekt som kan nulles (?), da må vi også tillate dette i databasen. 
//        Merk at IsComplete IKKE blir en kolonne. Den har ingen setter, den
//        regnes ut fra CompletedAt, og da har EF ingenting å lagre eller lese.
//        Modellen er "dyp" i den forstand at den ikke bare kjenner navnene,
//        men datatyper, nullability, nøkler, indekser og relasjoner mellom
//        tabeller. Det er den kunnskapen som gjør at EF kan skrive SQL for oss.
//
//Steg 2: Pakken Microsoft.EntityFrameworkCore.Design (se WebApi.csproj) er
//        maskineriet som bygger den modellen utenfor en kjørende app, og som
//        kan skrive C#-kode ut fra den. Den er markert PrivateAssets="all",
//        altså en ren verktøy-avhengighet. Det er kode som kun eksisterer i vårt prosjekt,
//        og er ikke en del av koden som blir kompilert ned til programmet vi faktisk kjører. 
//
//Steg 3: Kommandolinjeverktøyet dotnet-ef er verktøyet vi bruker for å få vårt design
//        over til vår faktiske database. Det er ikke installert globalt på maskinen, men låst til
//        prosjektet i dotnet-tools.json (et "tool manifest"), slik at alle på
//        teamet får nøyaktig samme versjon:
//
//              dotnet tool restore
//                  Henter ned dotnet-ef i den versjonen manifestet sier.
//                  Gjøres én gang etter at man har klonet repoet.
//
//              dotnet ef migrations add InitialCreate
//                  Starter appen vår i "design-modus" for å finne contexten og
//                  bygge modellen, sammenligner den mot forrige tilstand, og
//                  skriver differansen som C#-kode til Migrations/.
//
//              dotnet ef database update
//                  Kjører Up() i alle migrasjoner som ennå ikke er kjørt, mot
//                  den ekte databasen. Det var denne kommandoen som laget
//                  Data/TodoItemDatabase.db.
//
//              dotnet ef migrations remove
//                  Angrer den siste migrasjonen, så lenge du ikke har kjørt
//                  den mot databasen ennå.
//
//              dotnet ef database update <NavnPåForrigeMigrasjon>
//                  Ruller tilbake ved å kjøre Down() i motsatt rekkefølge.
//
//Migrations/-mappen er dermed generert kode, ikke håndskrevet, og skal
//sjekkes inn i git på lik linje med resten. Der ligger tre slags filer:
//      *_InitialCreate.cs           Up() bygger opp, Down() river ned igjen.
//      *_InitialCreate.Designer.cs  Modellen slik den så ut akkurat da.
//      TodoItemsContextModelSnapshot.cs
//                                   Modellen slik den ser ut NÅ. Det er denne
//                                   "add" sammenligner mot for å finne diffen,
//                                   og derfor den ene filen du virkelig ikke
//                                   vil redigere for hånd.
//Databasen holder selv orden på hva som er kjørt, i en egen tabell som heter
//__EFMigrationsHistory. Det er slik "database update" vet hva den kan hoppe over.
public class TodoItemsContext(DbContextOptions<TodoItemsContext> options) : DbContext(options), ITodoItemRepository
{
    //Dette er tabellen. En DbSet<T> er inngangen til én tabell, og oppfører seg
    //som en samling du kan skrive LINQ mot, men den er IKKE en liste i minnet.
    //Set<TodoItem>() henter samme objekt som den mer vanlige skrivemåten
    //  public DbSet<TodoItem> TodoItems { get; set; }
    //ville gitt oss. Fordelen med denne formen er at vi slipper en property som
    //kompilatoren tror kan være null, og som EF fyller inn "magisk" bak ryggen vår.
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoItem Add(TodoItem item)
    {
        //Add legger bare objektet i contextens huskeliste med state = Added.
        //Ingenting har skjedd i databasen ennå.
        TodoItems.Add(item);

        //Først her går det en INSERT over ledningen. SaveChanges pakker alle
        //ventende endringer inn i én transaksjon: enten går alt gjennom, eller
        //så går ingenting gjennom.
        SaveChanges();

        //Vi gir item tilbake selv om kalleren allerede har den.
        //Det er en vanlig konvensjon: hadde lagringen tildelt en id eller et
        //tidsstempel, ville dette vært måten kalleren fikk vite om det.
        //Med EF er dette mer enn en konvensjon: hadde Id vært en int som
        //databasen tildeler, ville SaveChanges skrevet den tilbake på objektet.
        return item;
    }

    public async Task<TodoItem> AddAsync(TodoItem item)
    {
        TodoItems.Add(item);
        await SaveChangesAsync();
        return item;
    }

    public TodoItem? Complete(Guid id)
    {
        //Denne linjen kjører ikke i C#, den blir oversatt til
        //  SELECT ... FROM TodoItems WHERE Id = @id LIMIT 1
        //og id sendes som en parameter, ikke limt inn i teksten. Det er derfor
        //EF gir oss beskyttelse mot SQL-injection nærmest gratis.
        //Sammenlign med minneversjonen i Core/TodoItemRepository: der lette vi
        //gjennom en liste i C#. Her gjør databasen jobben, og vi får én rad tilbake.
        var item = TodoItems.FirstOrDefault(i => i.Id == id);

        //Tidlig return, "guard clause". Alternativet er å pakke resten av
        //metoden inn i en if, og da vokser innrykket for hver sjekk du
        //legger til. Håndter det unormale først, så slipper hovedflyten fri.
        if (item is null) return item;

        item.Complete();
        //Legg merke til at vi aldri sier fra til EF om at vi endret noe.
        //Fordi denne spørringen IKKE hadde AsNoTracking, husker contexten
        //hvordan raden så ut da den ble lest. Ved SaveChanges sammenligner den
        //med hvordan objektet ser ut nå, og skriver en UPDATE med kun de
        //kolonnene som faktisk er forskjellige.
        SaveChanges();
        //Complete() er extension-metoden fra TodoItemExtensions.
        //Vi markerer den som Complete, og returnerer Itemen.
        return item;
    }

    public async Task<TodoItem?> CompleteAsync(Guid id)
    {
        //FirstOrDefaultAsync, ikke FirstOrDefault. Den asynkrone varianten
        //ligger i Microsoft.EntityFrameworkCore og finnes bare på spørringer
        //som faktisk går mot en database, det er jo bare da det er noe å vente på.
        var item = await TodoItems.FirstOrDefaultAsync(i => i.Id == id);
        if (item is null) return item;
        item.Complete();
        await SaveChangesAsync();
        return item;
    }

    //AsNoTracking: "les disse radene, men ikke husk dem".
    //Uten den ville contexten tatt vare på en kopi av hver eneste TodoItem for
    //å kunne oppdage endringer senere. Det koster minne og tid, og på en ren
    //leseoperasjon får vi aldri bruk for det, vi skal jo ikke lagre noe.
    //Regelen er enkel: skal du bare vise data, bruk AsNoTracking. Skal du endre
    //dem (som i Complete og Remove over), la sporingen være i fred.
    //
    //Merk også at det ikke går en eneste SQL-spørring når denne metoden
    //returnerer. Vi leverer tilbake en spørring som ennå ikke er kjørt, og den
    //kjører først når noen faktisk løper gjennom resultatet (her: når
    //controlleren serialiserer den til JSON). Det kalles deferred execution, og
    //det er verdt å kjenne til, for skjer det for sent, kan contexten allerede
    //være ryddet bort og du får en exception i fleisen.
    public IEnumerable<TodoItem> Get() => TodoItems.AsNoTracking();

    public TodoItem? Get(Guid id) => TodoItems.AsNoTracking().FirstOrDefault(i => i.Id == id);

    public async Task<List<TodoItem>> GetAsync()
    {
        //ToListAsync gjør det motsatte av linjen over: den kjører spørringen
        //her og nå, og gir oss en ferdig liste i minnet. Ingen overraskelser
        //senere, men til gjengjeld henter vi alt. Med en tabell på en million
        //rader vil du ha Skip/Take (paginering) her.
        return await TodoItems.AsNoTracking().ToListAsync();
    }

    public async Task<TodoItem?> GetAsync(Guid id)
    {
        return await TodoItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    }

    //Where blir til en WHERE-setning i SQL, ikke til filtrering i C#.
    //Forskjellen er ikke akademisk: databasen sender bare de radene som
    //matcher over nettverket, og kan bruke en indeks på CreatedAt til å finne
    //dem. Hadde vi hentet alt og filtrert med LINQ etterpå, hadde vi dratt hele
    //tabellen inn i minnet først.
    public IEnumerable<TodoItem> Range(DateTime to, DateTime from) => TodoItems.AsNoTracking().Where(i => i.CreatedAt >= from && i.CreatedAt <= to);

    public async Task<List<TodoItem>> RangeAsync(DateTime to, DateTime from) => await TodoItems.AsNoTracking().Where(i => i.CreatedAt >= from && i.CreatedAt <= to).ToListAsync();
    // Ovenfor oversatt vekk fra ekspressions:
    // public async Task<List<TodoItem>> RangeAsync(DateTime to, DateTime from)
    //{
    //  var found = TodoItems.AsNoTracking().Where(anyItem => anyItem.CreatedAt >= from, anyitem.CreatedAt <= to);
    //  return await found.ToListAsync();
    //
    //}

    public bool Remove(Guid id)
    {
        //To turer til databasen: én SELECT for å finne raden, og én DELETE.
        //Grunnen er at EF vil ha selve objektet å jobbe med. Trenger du å
        //slette mange rader effektivt, finnes ExecuteDelete som gjør det i én
        //spørring uten å laste noe inn i minnet.
        var result = TodoItems.FirstOrDefault(i => i.Id == id);

        //Fant vi ingenting, er det ikke en feil, det er bare et "nei".
        //Controlleren gjør dette om til 404, se ITodoItemRepository.
        if (result is null) return false;
        TodoItems.Remove(result);
        SaveChanges();
        return true;
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        var result = await TodoItems.FirstOrDefaultAsync(i => i.Id == id);
        if (result is null) return false;
        TodoItems.Remove(result);
        await SaveChangesAsync();
        return true;
    }

    //En liten observasjon til slutt: hver metode her kaller SaveChanges selv.
    //Det er lett å lese, men det betyr at hver operasjon blir sin egen
    //transaksjon. Skal to ting lykkes eller feile SAMMEN, må de dele ett
    //SaveChanges-kall, og da må ansvaret for å lagre flyttes ut av
    //repositoriet og opp til den som styrer arbeidsflyten.
}
