using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace HostaTests.Crypto
{
	[TestClass]
	public class RatchetCrypterTester
	{
		private RatchetCrypter a;
		private RatchetCrypter b;

		[TestInitialize]
		public void Constructor_Valid()
		{
			byte[] left = SecureRandomGenerator.GetBytes(RatchetCrypter.KEY_SIZE);
			byte[] right = SecureRandomGenerator.GetBytes(RatchetCrypter.KEY_SIZE);
			a = new RatchetCrypter(left, right);
			b = new RatchetCrypter(right, left);
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("S")]
		[DataRow("Medium length")]
		[DataRow("Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test!")]
		[DataRow("Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test!")]
		public void RoundTrip_Text(string plaintext)
		{
			byte[] plainblob = Encoding.UTF8.GetBytes(plaintext);

			// Test a->b
			byte[] package1 = a.Encrypt(plainblob);
			byte[] newPlainblob1 = b.Decrypt(package1);
			string newPlaintext1 = Encoding.UTF8.GetString(newPlainblob1);
			Assert.AreEqual(plaintext, newPlaintext1);

			// Test a->b again
			byte[] package2 = a.Encrypt(plainblob);
			byte[] newPlainblob2 = b.Decrypt(package2);
			string newPlaintext2 = Encoding.UTF8.GetString(newPlainblob2);
			Assert.AreEqual(plaintext, newPlaintext2);

			// Test b->a
			byte[] package3 = a.Encrypt(plainblob);
			byte[] newPlainblob3 = b.Decrypt(package3);
			string newPlaintext3 = Encoding.UTF8.GetString(newPlainblob3);
			Assert.AreEqual(plaintext, newPlaintext3);

			// Check that the packages are different
			CollectionAssert.AreNotEqual(package1, package2);
		}

		[TestMethod]
		public void RoundTrip_Empty()
		{
			byte[] plainblob = new byte[0];

			// Test a->b
			byte[] package1 = a.Encrypt(plainblob);
			byte[] newPlainblob1 = b.Decrypt(package1);
			CollectionAssert.AreEqual(plainblob, newPlainblob1);

			// Test a->b again
			byte[] package2 = a.Encrypt(plainblob);
			byte[] newPlainblob2 = b.Decrypt(package2);
			CollectionAssert.AreEqual(plainblob, newPlainblob2);

			// Test b->a
			byte[] package3 = a.Encrypt(plainblob);
			byte[] newPlainblob3 = b.Decrypt(package3);
			CollectionAssert.AreEqual(plainblob, newPlainblob3);

			// Check that the packages are different
			CollectionAssert.AreNotEqual(package1, package2);
		}
	}
}