//Kolleksjoner er måter for oss å samle sammen elementer i en felles samling.

//list, dictionary, array

using System.Collections;

//int[] numberArray = new(){1,2,3,4,5};
int[] numberArray = [1,2,3,4,5];
//List<int> numberlist = new(){1,2,3,4,5};
List<int> numberlist = [1,2,3,4,5];

numberArray[3] = 8;
//numberArray = numberArray.Append(9).ToArray();
numberArray = [..numberArray, 9];
numberlist.Add(19);

foreach (var n in numberArray) Console.WriteLine(n);

foreach (var n in numberlist) Console.WriteLine(n);

for (var i = 0; i < 4; i++)
{
    Console.WriteLine(numberlist[i]);
}

//Et dictionary er en annen måte å holde en samling av data på.
//Der kobler vi sammen en nøkkel (key) til en vilkårlig verdi (value), og kan
//bruke nøkkelen som en "index" for å hente ut verdien bak. 
Dictionary<string, int> nameAndAge = new(){
  ["John"] = 34
};

foreach (var keyValuePair in nameAndAge)
{
    Console.WriteLine(keyValuePair.Key);
    Console.WriteLine(keyValuePair.Value);
}

