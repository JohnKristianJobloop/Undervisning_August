//Interface Segregation Principle er at en klasse skal ikke tvinges
//til å implementere metoder fra en interface den egentlig ikke støtter.

public interface IMultiFunctionDevice: IPrintable, IScannable, IMailable;

public interface IPrintable
{
    void Print(string document);
}

public interface IScannable
{
    void Scan(string document);
}

public interface IMailable
{
    void SendEpost(string document);
}

public class AllInOnePrinter : IMultiFunctionDevice
{
    public void Print(string document) => Console.WriteLine($"Printed {document}");
    public void Scan(string document) => Console.WriteLine($"Scanned {document}");
    public void SendEpost(string document) => Console.WriteLine($"Sendt {document} to receiver");
}

public class SimpleLaserPrinter : IPrintable
{
    public void Print(string document) => Console.WriteLine($"Printed {document} on laser printer");
}



public static class Program
{
    public static void Main()
    {
        var multifunction = new AllInOnePrinter();
        var printer = new SimpleLaserPrinter();
        multifunction.Scan("Hello");
        multifunction.Print("Hi!");
        printer.Print("Hello");
    }
}