//Liskov Substitution Principle Sier at en superklasse,
//må kunne fungere som baseklassen sin. 


//Vi så at Bird, før vi introduserte Avian, tvang penguin til å implementere og override Fly().
//Det tvang Penguin til å throwe, aka ikke bli behandlet som en faktisk bird. 
//og det bryter med LSP
public class Avian
{
    
}
public class Bird: Avian
{
    public virtual void Fly() => Console.WriteLine("Bird is flying!");
}

public class Eagle : Bird
{
    public override void Fly() => Console.WriteLine("Eagly on the hunt!");
}

public class Penguin : Avian
{
    //public override void Fly() => throw new NotImplementedException("Penguins can't fly")
}

public static class BirdFlyer
{
    public static void MakeBirdFly(Bird bird) => bird.Fly();
}

public static class Program
{
    public static void Main()
    {
        var eagle = new Eagle();
        var penguin = new Penguin();
        BirdFlyer.MakeBirdFly(eagle);
        //BirdFlyer.MakeBirdFly(penguin);
    }
}