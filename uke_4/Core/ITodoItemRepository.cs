namespace Core;

//Her er D-en i SOLID, Dependency Inversion.
//Controlleren skal ikke vite at todoene våre ligger i en List<TodoItem> i
//minnet. 
//Controlleren vår er en typisk "high-order" klasse, Repositoriet en "low-order".
//Den skal bare vite at "det finnes noen som kan lagre og hente todos".
//Det er dette interfacet.
//
//Vi skal senere se ut at vi kan bytte ut hele vårt repository med en representasjon av et databasekall,
//og ingenting i controlleren må endre seg. 
//
//Det er også dette som gjør enhetstesting mulig: i en test sender vi inn en
//fake implementasjon av ITodoItemRepository, og slipper database helt.
public interface ITodoItemRepository
{
    TodoItem Add(TodoItem item);
    Task<TodoItem> AddAsync(TodoItem item);

    //bool, ikke void: kalleren må få vite om noe faktisk ble slettet.
    //Det er den forskjellen som avgjør om endepunktet skal svare 204 eller 404.
    bool Remove(Guid id);
    Task<bool> RemoveAsync(Guid id);

    //Overloading: to metoder med samme navn, men ulik signatur.
    //Get() henter alle, Get(id) henter én bestemt.
    IEnumerable<TodoItem> Get();
    Task<List<TodoItem>> GetAsync();

    //? betyr at metoden har LOV til å returnere null. Med
    //<Nullable>enable</Nullable> i csproj advarer kompilatoren kalleren
    //hvis den glemmer å håndtere det.
    //Fravær av ? er dermed et løfte: "du får aldri null herfra".
    TodoItem? Get(Guid id);
    Task<TodoItem?> GetAsync(Guid id);
    
    IEnumerable<TodoItem> Range(DateTime to, DateTime from);
    Task<List<TodoItem>> RangeAsync(DateTime to, DateTime from);

    TodoItem? Complete(Guid id);
    Task<TodoItem?> CompleteAsync(Guid id);
}
