using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace HostaTests.Crypto
{
	[TestClass]
	public class HasherTester
	{
		private readonly string[] testStrings = new string[] {
			"",
			"s",
			"medium",
			"long text to hash so that we can ensure no collisions"
		};

		[TestMethod]
		public void Hash_ConsistentButNoCollisions()

		{
			List<byte[]> hashes = new List<byte[]>();
			foreach (string s in testStrings)
			{
				byte[] bytes_original = Encoding.UTF8.GetBytes(s);
				byte[] bytes_clone = bytes_original.Clone() as byte[];
				byte[] original_hash = Hasher.Hash(bytes_original);
				byte[] clone_hash = Hasher.Hash(bytes_clone);
				CollectionAssert.AreEqual(original_hash, clone_hash);
				foreach (byte[] h in hashes)
				{
					CollectionAssert.AreNotEqual(original_hash, h);
				}
				hashes.Add(original_hash);
			}
		}

		[TestMethod]
		public void HMAC_ConsistentButNoCollisions()
		{
			List<byte[]> hmacs = new List<byte[]>();
			for (int i = 0; i < 10; i++)
			{
				byte[] key = SecureRandomGenerator.GetBytes(32);

				foreach (string s in testStrings)
				{
					byte[] bytes_original = Encoding.UTF8.GetBytes(s);
					byte[] bytes_clone = bytes_original.Clone() as byte[];
					byte[] original_hash = Hasher.HMAC(bytes_original, key);
					byte[] clone_hash = Hasher.HMAC(bytes_clone, key);
					CollectionAssert.AreEqual(original_hash, clone_hash);
					foreach (byte[] h in hmacs)
					{
						CollectionAssert.AreNotEqual(original_hash, h);
					}
					hmacs.Add(original_hash);
				}
			}
		}
	}
}