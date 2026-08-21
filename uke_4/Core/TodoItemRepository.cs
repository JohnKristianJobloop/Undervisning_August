namespace Core;


//Den konkrete implementasjonen: "in-memory", altså alt ligger i en helt vanlig
//List så lenge prosessen kjører. Restarter du serveren, er dataene borte.
//
//Det er helt greit i undervisning og i tester, og det er nettopp derfor
//interfacet er verdt bryet: den dagen vi bytter dette ut med SQL, er denne
//filen den ENESTE som må skrives om.
public class TodoItemRepository
{
    //"Databasen" vår. [] er en collection expression (C# 12), kortform for
    //new List<TodoItem>().
    //private fordi ingen utenfor klassen skal få rote i listen direkte, de må
    //gå gjennom metodene våre. 
    //Dette er det som heter inkapsling. Vi bestemmer selv hva som er lov å gjøre med vår liste
    //utenfor vår Repositoryimplementasjon.
    private List<TodoItem> _items{get;set;} = [];

    public TodoItem Add(TodoItem item)
    {
        _items.Add(item);
        //Vi gir item tilbake selv om kalleren allerede har den.
        //Det er en vanlig konvensjon: hadde lagringen tildelt en id eller et
        //tidsstempel, ville dette vært måten kalleren fikk vite om det.
        return item;
    }

    public bool Remove(Guid id)
    {
        //RemoveAll returnerer ANTALLET som ble fjernet. Vi oversetter det til
        //en bool, "ble noe faktisk slettet?", som er det svaret kalleren
        //trenger for å velge mellom 204 No Content og 404 Not Found.
        //Lambdaen i => i.Id == id er predikatet: den kjøres for hvert element.
        return _items.RemoveAll(i => i.Id == id) > 0;
    }

    //Merk returtypen: IEnumerable<TodoItem>, ikke List<TodoItem>.
    //Kalleren får lov til å iterere, ikke til å kalle
    //Add eller Clear. 
    public IEnumerable<TodoItem> Get() => [.. _items];

    //FirstOrDefault gir null hvis ingen matcher.
    //Siden vi bruker FirstOrDefault, og apiet potensielt gir oss en nullable verdi,
    //må vi bestemme hvordan vi håndterer det. Vi velger å bare passe det videre.
    //"Hvorfor ikke _items.First?"
    //First ville kastet exception.
    //"Fant ikke" er en del av vanlig programflyt, ikke en feil, og da skal vi
    //ikke bruke exceptions til å styre flyten.
    public TodoItem? Get(Guid id) => _items.FirstOrDefault(i => i.Id == id);

    //LINQ Where med et predikat, samme idé som i RemoveAll over.
    //Legg merke til at Where er LAT (lazy): ingenting filtreres i det denne
    //linjen kjører. Selve gjennomgangen skjer først når noen faktisk itererer
    //over resultatet, f.eks. i en foreach eller ved .ToList().
    public IEnumerable<TodoItem> Range(DateTime to, DateTime from) => _items.Where(i =>
                                                                                    i.CreatedAt >= from
                                                                                    && i.CreatedAt <= to );

    public TodoItem? Complete(Guid id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);

        //Tidlig return, "guard clause". Alternativet er å pakke resten av
        //metoden inn i en if, og da vokser innrykket for hver sjekk du
        //legger til. Håndter det unormale først, så slipper hovedflyten fri.
        if (item is null) return item;

        //Complete() er extension-metoden fra TodoItemExtensions.
        //Vi markerer den som Complete, og returnerer Itemen.
        return item.Complete();
    }
    //Eksempel av koden ovenfor som en expression method:
    /*
    *
    * public TodoItem? Complete(Guid id) => _items.FirstOrDefault(i => i.Id == id) is TodoItem item ? item.Complete() : null;
    *
    */
}
