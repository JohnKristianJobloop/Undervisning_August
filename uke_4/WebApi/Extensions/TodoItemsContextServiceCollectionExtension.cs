using Core;
using Microsoft.EntityFrameworkCore;
using WebApi.DatabaseContext;

namespace WebApi.Extensions;

//Program.cs skal kunne leses ovenfra og ned som en oppskrift. Hver gang
//oppsettet av en ting begynner å ta mer enn et par linjer, flytter vi det ut
//hit og gir det et navn som sier hva det gjør. I Program.cs står det da bare:
//      builder.Services.AddTodoItemDbContext(builder.Configuration);
//
//Dette er et fint mønster for å holde Program.cs så ryddig og lesbar som mulig. AddControllers() og AddOpenApi() er
//nøyaktig det samme, extension-metoder på IServiceCollection, bare skrevet av
//Microsoft. Konvensjonen er AddXxx for tjenester som registreres FØR Build(),
//og UseXxx for middleware som settes opp ETTER Build().
//
//Kravene er de samme som for TodoItemExtensions: klassen må være static, og
//navnet bør si hva den utvider.
public static class TodoItemsContextServiceCollectionExtension
{
    //Dette er den nye skrivemåten for extension members (C# 14): vi sier en
    //gang hvilken type vi utvider, og kan legge flere medlemmer inni blokken.
    //Den klassiske formen, som du vil se i all eldre kode og i det meste av
    //dokumentasjon, gjør det samme med et this-nøkkelord på første parameter:
    //
    //      public static IServiceCollection AddTodoItemDbContext(
    //          this IServiceCollection collection, IConfiguration config)
    //
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddTodoItemDbContext(IConfiguration config)
        {
            //AddDbContext gjør to registreringer for oss:
            //  1. "Ber noen om en ITodoItemRepository, gi dem en TodoItemsContext"
            //     Merk at det er interfacet som registreres. Controlleren
            //     kjenner fortsatt ikke til EF Core i det hele tatt.
            //  2. Den bygger DbContextOptions<TodoItemsContext> ut fra lambdaen
            //     under, og sender den inn i konstruktøren til contexten vår.
            //
            //Livssyklusen er Scoped som standard, altså ett objekt per
            //HTTP-request. Det er ikke tilfeldig, og det står i skarp kontrast
            //til AddSingleton-linjen vi hadde for minne-repositoriet:
            //      - En DbContext bærer på change tracking-tilstand som hører
            //        til ÉN brukers arbeid. Deles den, ser brukere hverandres
            //        halvferdige endringer.
            //      - En DbContext er ikke trådsikker. En singleton ville fått
            //        to samtidige requests inn i seg og krasjet.
            //      - Databasen er nå det som husker for oss, så contexten
            //        trenger ikke å overleve requesten. Den ryddes bort til
            //        slutt, og forbindelsen leveres tilbake til connection-poolen.
            collection.AddDbContext<ITodoItemRepository, TodoItemsContext>(
                opts =>
                {
                    //UseSqlite er provideren, den delen som kan SQLite sin
                    //dialekt og oversetter LINQ-treet til akkurat den SQL-en.
                    //Skal vi over på noe annet, byttes denne ene linjen ut med
                    //UseNpgsql (PostgreSQL) eller UseSqlServer, og pakken i
                    //csproj følger etter. Resten av koden vår står urørt.
                    //
                    //Connection stringen henter vi fra konfigurasjonen, ikke
                    //fra en hardkodet streng, og GetConnectionString leser
                    //seksjonen "ConnectionStrings:DefaultConnection" i
                    //appsettings.json. Grunnene til det er to:
                    //      - Ulikt miljø, ulik database. appsettings.Development.json
                    //        overstyrer appsettings.json når vi kjører lokalt.
                    //      - En ekte connection string inneholder brukernavn og
                    //        passord, og slikt skal ALDRI sjekkes inn i git.
                    //        I produksjon kommer den fra miljøvariabler eller en
                    //        secret store, og da uten at koden merker forskjell.
                    //Hos oss peker den på en fil: Data/TodoItemDatabase.db.
                    //SQLite er en hel database i én fil, uten server å
                    //installere, og derfor perfekt til undervisning.
                    opts.UseSqlite(config.GetConnectionString("DefaultConnection"));
                }
            );

            //Vi returnerer collection tilbake for å kunne kjede kall:
            //      services.AddTodoItemDbContext(config).AddControllers();
            //Det er samme fluent-stil som resten av ASP.NET bruker. Ikke
            //nødvendig, men det er slik en leser forventer at en AddXxx ser ut.
            return collection;
        }
    }

    //Bonus: det er også denne registreringen dotnet-ef bruker når vi kjører
    //"dotnet ef migrations add" eller "dotnet ef database update". Verktøyet
    //starter Program.cs i design-modus, finner DbContexten i DI-containeren,
    //og leser av både provideren og connection stringen herfra. Det er derfor
    //migrasjonene havner i nøyaktig samme databasefil som appen selv bruker.
}
