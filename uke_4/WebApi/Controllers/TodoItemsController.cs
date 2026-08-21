namespace WebApi.Controllers;
using Core;
using Microsoft.AspNetCore.Mvc;


//En controller samler endepunktene som hører til en ressurs, her TodoItems.
//Jobben dens er å ta imot HTTP, delegere arbeidet videre, og oversette svaret
//til en statuskode. Ser du forretningslogikk krype inn i en controller, hører
//den som regel hjemme i Core.

//[ApiController] slår på oppførsel vi ellers måtte skrevet selv:
//      - automatisk 400 Bad Request hvis en requestmodell ikke validerer
//      - [FromBody] blir underforstått for komplekse typer
//      - feilsvar formateres som ProblemDetails
[ApiController]

//Attributt-basert ruting. [Controller] er en placeholder som byttes ut med
//klassenavnet minus "Controller"-suffikset, altså blir ruten /TodoItems/.
//Poenget med placeholderen er at ruten følger med hvis klassen døpes om.
[Route("/[Controller]/")]

//Primary constructor igjen, men her er det DI-containeren som fyller inn
//parameterne for oss. Vi ber om ITodoItemRepository
//og TodoItemBuilder, og containeren slår opp registreringene fra Program.cs.
//En vanlig exception som kan oppstå, er at du ber om en tjeneste som ikke 
//er registrert i DI-containeren. Det vil throwe en exception.
public class TodoItemsController(ITodoItemRepository repository, TodoItemBuilder builder) : ControllerBase
{
    //GET /TodoItems/
    //Vi returnerer objektene rett ut, uten å pakke dem i Ok(). Rammeverket
    //serialiserer til JSON og setter 200 helt av seg selv.
    //AspNet Core gjennomfører en implicitt jsonserialisering, samt pakker det i en implicitt OK().
    //
    //Her gjør vi teknisk sett også en fyfy. Vi lekker domenemodellen (hvordan dataen er representert internt i programmet)
    //ut av webapiet vårt. Best practise er å også lage DTO modeller for data UT, på samme måte som data INN. 
    [HttpGet]
    public async Task<List<TodoItem>> Get() => await repository.GetAsync();
    //Samme som metoden over, bare uten expressions
    /*
    * public IEnumerable<TodoItem> Get(){
    *   return repository.Get();
    * }
    */

    //GET /TodoItems/{id}
    //Ruten arver /TodoItems/ fra [Route] på klassen, og legger til id-en.
    //:guid er en route constraint: sender noen /TodoItems/whatever, matcher ikke
    //det denne ruten, og de får 404 uten at koden vår kjøres.
    //
    //Returtypen er IActionResult fordi vi må kunne VELGE statuskode: 200 hvis
    //vi fant den, 404 hvis ikke. 
    //
    //"is TodoItem item" er type pattern matching. Den gjør to ting:
    //sjekker at resultatet ikke er null, og gir oss variabelen
    //"item" som vi kan bruke videre i uttrykket.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) => await repository.GetAsync(id) is TodoItem item ? Ok(item) : NotFound($"Item with id {id} not in the repository");
    //Samme som metoden over, bare uten expression og patternmatching
    /*
    * public IActionResult Get(Guid id){
    *   var item = repository.Get(id);
    *   if (item is null) return NotFound($"Item with id {id} not in repository");
    *   return Ok(item);
    * }
    */

    //POST /TodoItems/
    //[FromBody] sier at dataene skal leses fra request-bodyen og deserialiseres
    //fra JSON til en PostTodoDTO. Med [ApiController] er dette underforstått
    //for komplekse typer, men det skader ikke å være eksplisitt.
    //Søskenattributtene er [FromRoute], [FromQuery], [FromHeader] og [FromForm].
    //
    //Legg merke til at vi tar imot DTO-en, ikke TodoItem: klienten får dermed
    //ingen mulighet til å bestemme Id, CreatedAt eller CompletedAt selv.
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostTodoDTO dto)
    {
        //Oversett fra transportformat til domenemodell
        var item = builder.FromDto(dto);
        //Lagre item objektet. Merk hvor lite denne metoden egentlig gjør selv.
        await repository.AddAsync(item);

        //201 Created, ikke 200 OK. Created setter i tillegg en Location-header
        //som forteller klienten HVOR den nyopprettede ressursen ligger.
        //
        //En ting å merke seg her:
        //  - CreatedAtAction(nameof(GetAsync), new { id = item.Id }, item) er et mer ry da bygger ruteren URL-en for oss, og den
        //    fortsetter å stemme selv om vi endrer [Route] senere.
        return Created($"/TodoItems/{item.Id}", item);
        //Et alternativ til denne returtypen her er å bruke en CreatedAtAction type, som kan bygge returntypen selv:
        //return CreatedAtAction(nameof(GetAsync), new { id = item.Id }, item) som automatisk bygger ruten for å finne itemen for oss. 
    }

    //På torsdag skal vi implementere følgende:
    //repositoriet kan allerede Remove, Complete og Range,
    //men ingen av delene har et endepunkt ennå. Naturlig neste steg er
    //DELETE /TodoItems/{id}, en PATCH for å fullføre en oppgave, og en GET med
    //query-parametere (?from=...&to=...) for datointervall.
}
