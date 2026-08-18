namespace Core;

//Extension members lar oss henge nye metoder på en type utenfra: enten fordi
//vi ikke eier typen (string, DateTime, IEnumerable...), eller fordi vi vil
//holde den ren. TodoItem er ren data ( også kalt en POCO klasse ), oppførselen legger vi her.
//
//Kravene for en extension klasse er at hovedklassen er Statisk.
//Navngivning av klassen bør forklare hva klassen extender.
public static class TodoItemExtensions
{
    //Dette er den NYE syntaksen fra C# 14 / .NET 10: en extension-blokk.
    //"item" er mottakeren, altså den TodoItem-en metodene blir kalt på.
    //Det kan være hvilket som helst vilkårlig TodoItem objekt, vi refererer til det som "item". 
    //
    //Før C# 14, laget vi extensions via statiske metoder med et this parameter:
    /*
    * public static TodoItem Complete(this TodoItem item)
    * {
    *     item.CompletedAt = DateTime.UtcNow;
    *     return item;
    * }
    */
    //Gevinsten med extensionsblokken fra C# 14 er at "this TodoItem item" ikke må gjentas på hver
    //eneste metode. Kompilatoren lager fortsatt helt vanlige statiske metoder
    //bak kulissene som tidligere, men vi slipper den repetisjonen selv. 
    //
    //Den nye formen kan i tillegg noe den gamle ikke kunne: extension
    //properties, operatorer og statiske medlemmer, ikke bare metoder.
    //Hvis du vil kan du lage en operator extension som lar det plusse sammen to TodoItems... Ikke at jeg ser en grunn!
    extension(TodoItem item)
    {
        public TodoItem Complete()
        {
            //Igjen UtcNow. Og legg merke til at det er dette settet som gjør
            //IsComplete true, vi har ingen egen bool å holde i sync.
            item.CompletedAt = DateTime.UtcNow;
            return item;
        }

        //Alle tre metodene returnerer item i stedet for void, og det er et
        //bevisst valg. Det gir oss chaining:
        //      todo.UpdateTitle("Handle").UpdateBody("Melk og brød").Complete();
        //Samme mønster som LINQ, og som builder.Services... i Program.cs.
        //
        //Men merk: vi ENDRER objektet og returnerer det samme objektet. Et
        //fluent API betyr ikke automatisk at noe er immutabelt, og den som
        //hadde en referanse til denne todoen fra før, ser endringen med en gang.
        public TodoItem UpdateBody(string text)
        {
            item.Body = text;
            return item;
        }
        public TodoItem UpdateTitle(string title)
        {
            item.Title = title;
            return item;
        }
    }
}
