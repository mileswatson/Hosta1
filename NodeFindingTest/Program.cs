using System;
using System.Threading.Tasks;

namespace NodeFindingTest
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Network network = Node.network;
			Console.WriteLine("creating...");
			network.Add(25);
			Console.WriteLine("waiting...");
			Task.Delay(3000).Wait();
			Console.WriteLine("testing...");
			network.TestAll();
		}
	}
}