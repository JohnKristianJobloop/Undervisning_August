int[] numberArray = [1,2,3,4,5,6,7,8,9];

Func<int, bool> partall = tall => tall % 2 == 0;

IEnumerable<int> partallSamling = numberArray.Where(partall);

foreach(var n in partallSamling) Console.WriteLine(n);