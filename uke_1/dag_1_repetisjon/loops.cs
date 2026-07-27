int[] numberArray = [1,2,3,4,5];

for (var i = 0; i < numberArray.Length; i++)
{
    Console.WriteLine(numberArray[i]);
}

for (var i = 6; i > -1; i--)
{
    Console.WriteLine(i);
}

for (var i = 2; i < 20; i += 2)
{
    Console.WriteLine(i);
}

//Du har full kontroll over hva incrementoren din skal starte som (var i = 0)
//Du har full kontroll over hva konstrainen for at loopen fortsetter er (i < numberarray.Length)
//Du har full kontroll over hvor mye i skal vokse (i += 2)
for (var i = 0; i < numberArray.Length; i += 2)
{
    //Console.WriteLine(i);
    Console.WriteLine(numberArray[i]);
}

for (var i = 0; i < 10; i-= 100000000)
{
    Console.WriteLine(i);
}



