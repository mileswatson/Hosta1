using Hosta.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace HostaTests.ToolsTests
{
	[TestClass]
	public class CryptoTester
	{
		[DataTestMethod]
		[DataRow("")]
		[DataRow("s")]
		[DataRow("medium length")]
		[DataRow("long text to encrypt so that multiple blocks are used in CBC mode - this is an important test!")]
		public void RoundTrip_Valid(string plaintext)
		{
			byte[] plainblob = Encoding.UTF8.GetBytes(plaintext);

			byte[] key = Crypto.SecureRandomBytes(32);
			byte[] iv = Crypto.SecureRandomBytes(16);

			byte[] cipherblob = Crypto.Encrypt(plainblob, key, iv);
			byte[] newPlainblob = Crypto.Decrypt(cipherblob, key, iv);

			string newPlaintext = Encoding.UTF8.GetString(newPlainblob);

			Assert.AreEqual(plaintext, newPlaintext);
		}

		[TestMethod]
		public void RoundTrip_Empty()
		{
			byte[] key = Crypto.SecureRandomBytes(32);
			byte[] iv = Crypto.SecureRandomBytes(16);

			byte[] cipherblob = Crypto.Encrypt(new byte[0], key, iv);
			byte[] newPlainblob = Crypto.Decrypt(cipherblob, key, iv);

			CollectionAssert.AreEqual(newPlainblob, new byte[0]);
		}

		[DataTestMethod]
		[DataRow(1000)]
		public void SecureRandomBytes_DifferentEverytime(int numToCompare)
		{
			List<byte[]> generated = new List<byte[]>();
			for (int i = 0; i < numToCompare; i++)
			{
				generated.Add(Crypto.SecureRandomBytes(8));
				for (int j = 0; j < i; j++)
				{
					CollectionAssert.AreNotEqual(generated[i], generated[j]);
				}
			}
		}
	}
}