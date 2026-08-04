using System.Text;

List<int> numbers = [1,3,4,5,2,8,456,23,56123,34,6578,345,967,23];

//I en Func, de første parameterene i krokodilleklammene, er input parameterdatatypen til funksjonen din
//Det siste parametere, er datatypen du returnerer. 
Func<int, bool> predicate = (num) => (num & 1) != 0;


Func<string, string, string> concatenate = (s1, s2) => s1 + s2;

Func<int, string> createWeirdString = num =>
{
    var buff = new byte[10];
    Random.Shared.NextBytes(buff);
    var builder = new StringBuilder();
    foreach(var b in buff) builder.Append((char)b);
    return num + builder.ToString();
};

Func<int, int> multiplier = num => num * 2;

var result = concatenate("Hello ", "World!");

var isSevenEven = predicate(7);

var partall = numbers.Where(num => (num & 1) == 0).Select(createWeirdString);

foreach (var p in partall) Console.WriteLine(p);


public static class NumberValidator
{
    public static bool IsEven(int num) => num % 2 == 0;
}