using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Hosta.Crypto;
using Hosta.Tools;

namespace NodeFindingTest
{
	internal static class Network
	{
		private static Dictionary<string, BitArray> addressToName = new Dictionary<string, BitArray>();
		private static Dictionary<BitArray, Node> nameToNode = new Dictionary<BitArray, Node>();

		public static void AddNodes(int num)
		{
			//NodeReference firstNode = null;
			for (int i = 0; i < num; i++)
			{
				Node n = new Node();
				addressToName[n.address] = n.name;
				nameToNode[n.name] = n;
				//n.BuildTable(firstNode);
				//if (i == 0) firstNode = n.Reference;

				if (nameToNode.Count == 1) n.BuildTable(null);
				else
				{
					NodeReference nr;
					do
					{
						nr = RandomReference();
					} while (nr.name == n.name);
					n.BuildTable(nr);
				}
				Console.WriteLine(i);
			}
		}

		public static bool IsValidReference(NodeReference nr)
		{
			if (nr == null || nr.name == null
				|| nr.address == null || !addressToName.ContainsKey(nr.address))
			{
				return false;
			}
			return addressToName[nr.address] == nr.name;
		}

		public static Node Get(NodeReference nf)
		{
			if (IsValidReference(nf))
			{
				return nameToNode[nf.name];
			}
			else
			{
				return null;
			}
		}

		public static NodeReference RandomReference()
		{
			return nameToNode.ElementAt(
				SecureRandomGenerator.GetInt(0, nameToNode.Count))
				.Value.Reference;
		}

		public static bool AreConnected(Node a, Node b)
		{
			if (a == null | b == null) Console.WriteLine("VALUE WAS NULL!");
			NodeReference nr = a.Find(b.name);
			if (nr == null) return false;
			return nr.Equals(b.Reference);
		}

		public static void TestAll()
		{
			int totalAttempts = 100;
			int totalConnections = 0;
			for (int i = 0; i < totalAttempts; i++)
			{
				Node a = nameToNode[RandomReference().name];
				Node b = nameToNode[RandomReference().name];
				if (AreConnected(a, b)) totalConnections++;
			}
			Console.WriteLine(totalConnections);
			Console.WriteLine(totalAttempts);
		}
	}
}