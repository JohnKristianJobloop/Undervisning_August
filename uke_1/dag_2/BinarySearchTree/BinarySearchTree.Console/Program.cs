using System.Diagnostics;
using BinarySearchTree.Core.Models;

List<int> numbers = [];
for (var i = 0; i < 100000; i++)
{
    numbers.Add(Random.Shared.Next());
}

var index = Random.Shared.Next(0, 100000 - 1);
numbers[index] = 52;

BinarySearchTree<int> tree = new();


foreach(var num in numbers) tree.Insert(num);

var stopwatch = Stopwatch.StartNew();
var found = tree.BinaryTreeSearch(52);
stopwatch.Stop();

Console.WriteLine($"52 in tree: {found.Item1}.\n It took {found.Item2} steps down the tree to find it.\n It took {stopwatch.ElapsedMilliseconds} to find it");

