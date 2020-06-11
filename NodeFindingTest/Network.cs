using System;
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
	internal class Network
	{
		private Dictionary<string, string> addressToID;
		private Dictionary<string, Node> idToNode;

		public Network()
		{
			addressToID = new Dictionary<string, string>();
			idToNode = new Dictionary<string, Node>();
		}

		public void Add(int num)
		{
			for (int i = 0; i < num; i++)
			{
				Node n = new Node();
				addressToID[n.reference.address] = n.reference.ID;
				idToNode[n.reference.ID] = n;
				if (idToNode.Count == 1) n.StartTableBuilder(new NodeReference());
				else
				{
					NodeReference nr;
					do
					{
						nr = RandomReference();
					} while (nr.ID == n.reference.ID);
					n.StartTableBuilder(nr);
				}
				Console.WriteLine(i);
				Task.Delay(150).Wait();
			}
		}

		public bool IsValidReference(NodeReference nf)
		{
			if (nf.ID == null || nf.address == null || !addressToID.ContainsKey(nf.address))
			{
				return false;
			}
			return addressToID[nf.address] == nf.ID;
		}

		public Node Get(NodeReference nf)
		{
			if (IsValidReference(nf))
			{
				return idToNode[nf.ID];
			}
			else
			{
				return null;
			}
		}

		public NodeReference RandomReference()
		{
			return idToNode.ElementAt(
				SecureRandomGenerator.GetInt(0, idToNode.Count))
				.Value.reference;
		}

		public bool CanConnect(string a, string b)
		{
			return idToNode[a].Find(b).address == idToNode[b].reference.address;
		}

		public void TestAll()
		{
			int totalAttempts = 100;
			int totalConnections = 0;
			var list = idToNode.Keys.ToList();
			for (int i = 0; i < totalAttempts; i++)
			{
				int a = SecureRandomGenerator.GetInt(0, list.Count);
				int b = SecureRandomGenerator.GetInt(0, list.Count);
				if (CanConnect(list[a], list[b])) totalConnections++;
			}
			Console.WriteLine(totalConnections);
			Console.WriteLine(totalAttempts);
		}
	}
}