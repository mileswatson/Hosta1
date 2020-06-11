using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hosta.Crypto;
using Hosta.Tools;

namespace NodeFindingTest
{
	internal class NodeReference : IEquatable<NodeReference>
	{
		public BitArray name;
		public string address;

		public NodeReference()
		{
			this.name = null;
			this.address = null;
		}

		public NodeReference(BitArray name, string address)
		{
			this.name = name;
			this.address = address;
		}

		public override string ToString()
		{
			byte[] placeholder = new byte[32];
			name.CopyTo(placeholder, 0);
			return "{" + Transcoder.HexFromBytes(placeholder) + ":" + address + "}";
		}

		public bool Equals(NodeReference nr)
		{
			return name.Equals(nr.name) && address == nr.address;
		}
	}

	internal class Bucket
	{
		private const int SIZE = 16;
		private List<NodeReference> main = new List<NodeReference>();
		private Queue<NodeReference> backup = new Queue<NodeReference>();

		public int Size {
			get { return main.Count; }
		}

		public Bucket()
		{
		}

		public void Cleanup()
		{
			for (int i = 0; i < main.Count; i++)
			{
				if (!Network.IsValidReference(main[i])) main.RemoveAt(i);
				i--;
			}
			while (main.Count != 16 && backup.Count > 0)
			{
				NodeReference nr = backup.Dequeue();
				if (Network.IsValidReference(nr)) main.Add(nr);
			}
		}

		public void AddReference(NodeReference newReference)
		{
			bool a = (main.Count < SIZE);
			bool b = true;
			foreach (NodeReference nr in main)
			{
				if (newReference.Equals(nr))
				{
					b = false;
					break;
				}
			}
			if (a && b)
			{
				main.Add(newReference);
			}
			else
			{
				foreach (NodeReference nr in backup)
				{
					if (newReference.Equals(nr)) return;
				}
				backup.Enqueue(newReference);
				if (backup.Count > SIZE) backup.Dequeue();
			}
		}

		public NodeReference GetClosest(BitArray name)
		{
			if (Size == 0) return null;
			bool first = true;
			BigInteger smallestDistance = BigInteger.Zero;
			NodeReference closestReference = new NodeReference();
			foreach (NodeReference nr in main)
			{
				BitArray clone = (BitArray)nr.name.Clone();
				clone.Xor(name);
				byte[] bytes = new byte[32];
				clone.CopyTo(bytes, 0);
				BigInteger size = new BigInteger(bytes, true, true);
				if (first)
				{
					smallestDistance = size;
					closestReference = nr;
					first = false;
				}
				else if (size < smallestDistance)
				{
					smallestDistance = size;
					closestReference = nr;
				}
			}
			return closestReference;
		}

		public List<NodeReference> GetAllReferences()
		{
			return main;
		}
	}

	internal class RoutingTable
	{
		public BitArray self;
		public Bucket[] buckets = new Bucket[256];

		public RoutingTable(BitArray self)
		{
			this.self = self;
			for (int i = 0; i < 256; i++)
			{
				buckets[i] = new Bucket();
			}
		}

		public void AddReference(NodeReference newReference)
		{
			int level = 0;
			while (self[level] == newReference.name[level])
			{
				level++;
				if (level == 256) return;
			}
			buckets[level].AddReference(newReference);
		}

		public NodeReference GetClosest(BitArray target)
		{
			int level = 0;
			while (self[level] == target[level])
			{
				level++;
				if (level == 256) return null;
			}
			return buckets[level].GetClosest(target);
		}

		public List<NodeReference> GetAllReferences()
		{
			List<NodeReference> all = new List<NodeReference>();
			foreach (Bucket bucket in buckets)
			{
				all.AddRange(bucket.GetAllReferences());
			}
			return all;
		}
	}

	internal class Node
	{
		private const int NAME_BITS = 32;

		public BitArray name;
		public string address;

		public RoutingTable table;

		public NodeReference Reference {
			get { return new NodeReference(name, address); }
		}

		public Node()
		{
			name = new BitArray(SecureRandomGenerator.GetBytes(32));
			address = Transcoder.HexFromBytes(SecureRandomGenerator.GetBytes(32));
			table = new RoutingTable(name);
		}

		public NodeReference GetClosest(BitArray name)
		{
			return table.GetClosest(name);
		}

		public void AddReference(NodeReference newReference)
		{
			table.AddReference(newReference);
		}

		public List<NodeReference> GetAllReferences()
		{
			return table.GetAllReferences();
		}

		public void BuildTable(NodeReference current)
		{
			while (true)
			{
				if (!Network.IsValidReference(current) || current.Equals(Reference)) return;
				Node n = Network.Get(current);
				foreach (NodeReference nr in n.GetAllReferences())
				{
					AddReference(nr);
				}
				current = n.GetClosest(name);
				n.AddReference(Reference);
			}
		}

		public NodeReference Find(BitArray target)
		{
			if (target.Equals(name)) return Reference;
			NodeReference current = GetClosest(target);
			while (Network.IsValidReference(current) && !current.Equals(Reference))
			{
				if (current.name.Equals(target)) return current;
				Node n = Network.Get(current);
				current = n.GetClosest(target);
			}
			return null;
		}
	}
}