//Single Responsibility Principle sier at klassen vi lager,
//skal kun ha en grunn for å endre tilstand. 



//Her bryter vi med SRP
public class BadEmployeeClass(string name, decimal hourlyRate, int hoursWorked)
{
    public string Name {get;} = name;
    public decimal HourlyRate{get;} = hourlyRate;
    public int HoursWorked{get;} = hoursWorked;

    public decimal CalculatePay()
    {
        var normalHours = (decimal)Math.Min(HoursWorked, 37.5);
        var overtime = (decimal)Math.Max(HoursWorked - 37.5, 0);
        return normalHours * HourlyRate + overtime * HourlyRate * 1.5m;
    }

    public void SaveToDatabase()
    {
        Console.WriteLine($"[SQL] INSERT INTO Employees(Name, Rate) VALUES ('{Name}, {HoursWorked}')");
    }

    public void SendPayslip()
    {
        Console.WriteLine($"[SMTP] Sender lønnslipp til {Name}: {CalculatePay():N2} kr");
    }
}

//Nedenfor korrigerer vi SRP
public record GoodEmployeeClass(string Name, decimal HourlyRate, int HoursWorked);

public class PayCalculator
{
    private const double NormalWeek = 37.5;
    private const decimal OvertimeFactor = 1.5m;

    public decimal CalculatePay(GoodEmployeeClass employee)
    {
        var normalHours = Math.Min(employee.HoursWorked, NormalWeek);
        var overtime = Math.Max(employee.HoursWorked - NormalWeek, 0);

        return (decimal)normalHours * employee.HourlyRate 
                + (decimal)overtime * employee.HourlyRate * OvertimeFactor;
    }
}

public class EmployeeRepository
{
    private readonly List<GoodEmployeeClass> _storage = [];

    public void Save(GoodEmployeeClass employee)
    {
        _storage.Add(employee);
    }
}

public class PaySlipMailService
{
    public void Send(GoodEmployeeClass employee, decimal pay)
    {
        Console.WriteLine($"[SMTP] Sender lønnslipp til {employee.Name}: {pay:N2} kr");
    }
}



public static class Program
{
    public static void Main()
    {
        var badEmployee = new BadEmployeeClass("John", 150, 40);
        badEmployee.SendPayslip();

        var goodEmployee = new GoodEmployeeClass("Terje", 230, 60);
        var mailer = new PaySlipMailService();
        var calculator = new PayCalculator();
        mailer.Send(goodEmployee, calculator.CalculatePay(goodEmployee));
    }
}