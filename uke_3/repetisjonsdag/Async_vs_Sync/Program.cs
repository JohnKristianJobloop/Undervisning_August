void CaclculateSumOfArray(long[] arr, out long result)
{
    result = arr.Sum();
}


long[] arr = [..Enumerable.Range(0,10000000).Select(n => (long)Random.Shared.Next())];

long[] arr2 = [..Enumerable.Range(0,10000000).Select(n => (long)Random.Shared.Next())];

long result = 0;
long result2 = 0;


//var thread1 = new Thread(()=>CaclculateSumOfArray(arr, out result));
//var thread2 = new Thread(()=>CaclculateSumOfArray(arr2, out result2));

//thread1.Start();
//thread2.Start();

//thread1.Join();
//thread2.Join();


var task = Task.Run(()=>CaclculateSumOfArray(arr, out result));
var task2 = Task.Run(()=>CaclculateSumOfArray(arr2, out result2));


await Task.WhenAll(task, task2);

Console.WriteLine($"result 1: {result}, result 2: {result2}");


