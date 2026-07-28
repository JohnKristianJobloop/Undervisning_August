using System.Collections;

List<int> numbers = [];
for (var i = 0; i < 10000000; i++)
{
    numbers.Add(Random.Shared.Next());
}

var index = Random.Shared.Next(0, 10000000 - 1);

numbers[index] = 48;

var found = numbers.Contains(48);



foreach (var n in numberList) Console.WriteLine(n.GetType());

ArrayList listOfStuff = ["Hello", 1, 123, 34.2f, "World!"];
foreach (var item in listOfStuff) Console.WriteLine(item.GetType());
