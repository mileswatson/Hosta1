using System;
using System.Threading.Tasks;

namespace NodeFindingTest
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("creating...");
			Network.AddNodes(10000);
			Console.WriteLine("testing...");
			Network.TestAll();
		}
	}
}