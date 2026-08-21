void CaclculateSumOfArray(string threadName, long[] arr, out long result)
{
    long sum = 0;
    for (var i = 0; i < arr.Length; i++)
    {
        Console.WriteLine($"{threadName} is Counting! {arr[i]}");
        sum += arr[i];
    }
    result = sum;
}


long[] arr = [..Enumerable.Range(0,1000).Select(n => (long)Random.Shared.Next())];

long[] arr2 = [..Enumerable.Range(0,1000).Select(n => (long)Random.Shared.Next())];

long result = 0;
long result2 = 0;


var thread1 = new Thread(()=>CaclculateSumOfArray("Thread 1", arr, out result));
var thread2 = new Thread(()=>CaclculateSumOfArray("Thread 2", arr2, out result2));

thread1.Start();
thread2.Start();

thread1.Join();
thread2.Join();


//var task = Task.Run(()=>CaclculateSumOfArray(arr, out result));
//var task2 = Task.Run(()=>CaclculateSumOfArray(arr2, out result2));


//await Task.WhenAll(task, task2);

Console.WriteLine($"result 1: {result}, result 2: {result2}");


