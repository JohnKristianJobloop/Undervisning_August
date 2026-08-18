//Filen har to faser, og skillet mellom dem er viktigere enn det ser ut:
//      1. REGISTRERE tjenester   -> alt før builder.Build()
//      2. SETTE OPP pipelinen    -> alt etter builder.Build()
//Prøver du å registrere en tjeneste etter Build(), får du exception.

//Konfigurering og oppsett av App
using Core;

//WebApplicationBuilder samler tre ting på ett sted: konfigurasjon
//(appsettings.json, miljøvariabler, kommandolinjeargumenter), logging, og
//DI-containeren (builder.Services).
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//OpenAPI leser controllerne våre med refleksjon og genererer en maskinlesbar
//beskrivelse av API-et. Det er den beskrivelsen Swagger-siden tegner opp.
builder.Services.AddOpenApi();

//Her kobles interface til implementasjon i
//Dependency Injection: "når noen ber om en ITodoItemRepository, gi dem en
//TodoItemRepository". Controlleren spør bare etter interfacet, og trenger
//aldri å vite hva objekt Dependency Injection Containeren faktisk gir inn.
//
//Livssyklusen bestemmer HVOR LENGE objektet lever:
//      Singleton -> ETT objekt for hele appen, delt av alle brukere
//      Scoped    -> ett objekt per HTTP-request (typisk valg for en DbContext)
//      Transient -> et helt nytt objekt hver eneste gang noen spør
//
//At det står Singleton her er ikke tilfeldig, det er nødvendig: lista med
//todos ligger i minnet PÅ selve repository-objektet. Hadde vi valgt Scoped
//eller Transient, ville hver request fått en ny, tom liste, og alt du la inn
//ville vært borte i det neste kallet. Prøv det gjerne, det er en lærerik feil.
//
//Prisen for Singleton er at flere requests kan treffe samme List samtidig, og
//List<T> er ikke trådsikker. I produksjon måtte vi hatt en lås, en
//concurrent-collection, eller rett og slett en ekte database.
//
//Vi skal se på concurrency og asynkronitet på torsdag. 
builder.Services.AddSingleton<ITodoItemRepository, TodoItemRepository>();

//Builderen har ingen tilstand å ta vare på, så det koster oss ingenting å
//lage en ny per bruk.
//Merk at vi registrerer den KONKRETE klassen, uten interface foran. Det er
//fullt lovlig, men da har vi ingen sømmer å bytte den ut i, verken i en test
//eller senere i livet. Sammenlign med linjen ovenfor. Vi er låst til kun en implementasjon av TodoItemBuilder.
builder.Services.AddTransient<TodoItemBuilder>();

//Finner alle klasser som arver ControllerBase og gjør dem kjent for rammeverket.
//Uten denne linjen blir controlleren vår aldri funnet, og alt svarer 404.
builder.Services.AddControllers();


//Baker konfigurering, får ut en fullstendig webserver
//Etter dette punktet er DI-containeren låst. Nå bygger vi request-pipelinen.
var app = builder.Build();

// Configure the HTTP request pipeline.
//app.Environment styres av miljøvariabelen ASPNETCORE_ENVIRONMENT, som settes
//i Properties/launchSettings.json når du kjører lokalt.
//Swagger er et utviklerverktøy og er potensielt noe du ikke vil eksponere i det ferdige produktet ditt, derfor
//ligger det inne i denne if-en.
if (app.Environment.IsDevelopment())
{
    //Selve JSON-dokumentet som beskriver API-et.
    app.MapOpenApi();

    //Den klikkbare testsiden oppå det dokumentet. Kjør appen og gå til /swagger.
    app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/openapi/v1.json", "Todo App v1"));
}

//Slik så endepunktene ut FØR vi flyttet dem inn i en controller.
//Minimal API og controllere gjør nøyaktig det samme; forskjellen er
//organisering. Minimal API er kjappest for noen få ruter, controllere holder
//orden når antallet vokser og ting skal grupperes per ressurs.
//Legg merke til at repositoriet kom inn som en parameter også der, DI virker
//i begge stilartene.
//app.MapGet("TodoItems", (ITodoItemRepository repository)=> repository.Get());

//app.MapGet("TodoItems/{id:guid}", (ITodoItemRepository repository, Guid id) => repository.Get(id));

//Middleware. Rekkefølgen på disse linjene er ikke pynt, den ER programflyten:
//hver request faller nedover gjennom pipelinen, og svaret bobler tilbake opp
//gjennom den samme rekka i motsatt retning.
//Denne svarer med en omdirigering til https hvis noen banker på med http.
app.UseHttpsRedirection();

//Siste ledd i pipelinen: finn den controller-metoden som matcher en vilkårlig rute, og
//kjør den. Kommer man hit uten treff, blir det 404.
app.MapControllers();


//Starter Kestrel (webserveren) og blokkerer her til appen avsluttes.
//Alt over har bare vært oppsett. Det er først nå noen faktisk lytter på porten.
app.Run();
