//Open Closed Principle sier at en klasse skal være åpen for extensions
//Men ikke for modifisering. 

public enum CustomerType
{
    Regular = 100,
    Member = 90,
    Employee = 70,
    Student = 50,
    Honør = 49
}

public class DiscountCalculator()
{
    public decimal ApplyDiscount(decimal price, CustomerType type) => 
        price * (decimal)type/100m;
}