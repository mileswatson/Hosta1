using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Hosta.Crypto;
using Hosta.Tools;

namespace NodeFindingTest
{
	internal struct NodeReference
	{
		public string ID;
		public string address;

		public override string ToString()
		{
			return "{" + ID + ":" + address + "}";
		}
	}

	internal class Node
	{
		private const int ID_BYTES = 32;
		public static Network network = new Network();

		private static char[] hexchars = "0123456789abcdef".ToCharArray();

		public NodeReference reference;
		public NodeReference[,] table = new NodeReference[ID_BYTES, 16];

		public ConcurrentQueue<NodeReference> newNodeQueue = new ConcurrentQueue<NodeReference>();

		public Node()
		{
			byte[] temp = SecureRandomGenerator.GetBytes(ID_BYTES);
			reference.ID = Transcoder.HexFromBytes(temp);
			reference.address = Transcoder.HexFromBytes(SecureRandomGenerator.GetBytes(32));
			for (int i = 0; i < ID_BYTES; i++)
			{
				int j = Array.FindIndex(hexchars, c => c == reference.ID[i]);
				table[i, j] = reference;
			}
		}

		public NodeReference GetClosestReference(string ID)
		{
			for (int i = 0; i < ID_BYTES; i++)
			{
				if (ID[i] == reference.ID[i]) continue;
				int index = Array.FindIndex(hexchars, c => c == ID[i]);
				return table[i, index];
			}
			return reference;
		}

		public NodeReference Find(string ID)
		{
			Node n = this;
			while (n != null)
			{
				if (n.reference.ID == ID) return n.reference;
				n = network.Get(n.GetClosestReference(ID));
			}
			return new NodeReference();
		}

		private void CleanupTable()
		{
			for (int i = 0; i < ID_BYTES; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if (table[i, j].ID != null
						&& table[i, j].ID != reference.ID
						&& !network.IsValidReference(table[i, j]))
					{
						table[i, j].ID = null;
						table[i, j].address = null;
					}
				}
			}
		}

		public void AddToTable(NodeReference nr)
		{
			for (int i = 0; i < ID_BYTES; i++)
			{
				if (nr.ID[i] == reference.ID[i]) continue;
				int index = Array.FindIndex(hexchars, c => c == nr.ID[i]);
				if (table[i, index].ID != null) table[i, index] = nr;
			}
		}

		public async void StartTableBuilder(NodeReference current)
		{
			while (current.ID != null && current.address != null)
			{
				Node n = network.Get(current);
				Console.WriteLine(current);
				n.AddToTable(reference);
				int level = 0;
				do
				{
					if (level == ID_BYTES / 2) break;
					int skipVal = Array.FindIndex(hexchars, c => c == reference.ID[level]);
					for (int value = 0; value < 16; value++)
					{
						if (value == skipVal) continue;
						if (n.table[level, value].ID != null)
						{
							table[level, value] = n.table[level, value];
						}
					}
					level++;
				} while (reference.ID[level - 1] == n.reference.ID[level - 1]);
				current = n.GetClosestReference(reference.ID);
				if (current.ID == n.reference.ID) break;
			}
			while (true)
			{
				Queue<NodeReference> builderQueue = new Queue<NodeReference>();
				CleanupTable();
				foreach (NodeReference nr in table)
				{
					if (nr.ID != null && nr.ID != reference.ID)
						builderQueue.Enqueue(nr);
				}
				while (builderQueue.Count > 0)
				{
					var n = network.Get(builderQueue.Dequeue());
					if (n == null) continue;
					int level = 0;
					do
					{
						for (int value = 0; value < 16; value++)
						{
							if (table[level, value].ID == null)
							{
								NodeReference nr = n.table[level, value];
								if (nr.ID != null)
								{
									table[level, value] = nr;
									network.Get(nr).AddToTable(reference);
								}
							}
						}
						level++;
					} while (reference.ID[level - 1] == n.reference.ID[level - 1]);
				}
				await Task.Delay(SecureRandomGenerator.GetInt(100, 150));
			}
		}
	}
}