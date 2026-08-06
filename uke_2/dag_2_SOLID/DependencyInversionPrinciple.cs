//Dependency Inversion Principle er at Lav-Nivå tjenester, skal ikke svære
//avhengig av Høynivå tjenester. 

//En typisk lavnivåtjeneste er en del av koden din med et konkret arbeidsansvar knyttet til en spesifikk operasjon. 
//Det er typisk en klasse som representerer instrukser for å lagre data til en database.
//Lese filer osv. 
public class SqlDatabase
{
    private List<string> _orders = [];
    public void Save(string order) => _orders.Add(order);
}

public class EmailSender()
{
    public void Send(string to, string from, string content) => Console.WriteLine($"SENT {content} to {to} from {from}");
}


//En typisk høynivå tjeneste, delegerer bare arbeid videre til andre lavnivåtjenester i programmet ditt. 
//Denne ordretjenesten bryter Dependency Inversion prinsippet, siden den tar eierskap over SqlDatabasen, og EmailSenderen. 
//En høynivåtjeneste, skal ikke ha ansvaret for Livetimen (hvor lenge objektet eksisterer i programmet ditt) for lavnivå objekter. 
public class BadOrderService
{
    private SqlDatabase _repository = new();
    private EmailSender _sender = new();

    public void PlaceOrder(string order, string customerEmail)
    {
        _repository.Save(order);
        _sender.Send(customerEmail, "Workspace@SuperAmazon.com", order);
    }
}


//Her har vi gjort OrderServices avhengig av SqlDatabasen og Emailsenderen vår, 
//og ikke omvendt. 
public class OrderService(SqlDatabase database, EmailSender mailer)
{
    public void PlaceOrder(string order, string customerEmail)
    {
        database.Save(order);
        mailer.Send(customerEmail, "Workspace@SuperAmazon.com", order);
    }
}



//Det er ofte lurt at den laveste klassen i programmet ditt (i dette tilfellet Program.Main()) har ansvaret
//for Lifetimen til objektene du lager. 
public static class Program
{
    public static void Main()
    {
        var database = SqlDatabase();
        var mailer = EmailSender();
    }

    private void CreateBadOrder()
    {
        var service = new BadOrderService();
        var order = "Ny ordre!";

        service.PlaceOrder(order);
    }
    private void CreateOrder(SqlDatabase database, EmailSender sender)
    {
        var service = new OrderService(database, sender);
        var order = "Ny Ordre!";
        service.PlaceOrder(order);
    }
}
