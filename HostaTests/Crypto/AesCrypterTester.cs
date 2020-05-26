using Microsoft.VisualStudio.TestTools.UnitTesting;

using Hosta.Crypto;
using Hosta.Exceptions;
using System.Text;
using System;

namespace HostaTests.Crypto
{
	/*
	[TestClass]
	public class AesCrypterTester
	{
		[TestMethod]
		public void Constructor_Valid()
		{
			new RatchetCrypter(new byte[RatchetCrypter.KEY_SIZE]);
		}

		[TestMethod]
		public void Constructor_InvalidKey()
		{
			Assert.ThrowsException<CryptoParameterException>(
				() => new RatchetCrypter(new byte[0])
			);
			Assert.ThrowsException<CryptoParameterException>(
				() => new RatchetCrypter(new byte[31])
			);
			Assert.ThrowsException<CryptoParameterException>(
				() => new RatchetCrypter(new byte[33])
			);
			Assert.ThrowsException<CryptoParameterException>(
				() => new RatchetCrypter(new byte[64])
			);
		}

		[TestMethod]
		public void Encrypt_InvalidIV()
		{
			byte[] key = SecureRandomGenerator.GetBytes(32);
			using var se = new RatchetCrypter(key);

			byte[] plainblob = SecureRandomGenerator.GetBytes(100);

			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Encrypt(plainblob, new byte[0])
			);
			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Encrypt(plainblob, new byte[RatchetCrypter.IV_SIZE - 1])
			);
			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Encrypt(plainblob, new byte[RatchetCrypter.IV_SIZE + 1])
			);
			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Encrypt(plainblob, new byte[RatchetCrypter.IV_SIZE * 2])
			);
		}

		[TestMethod]
		public void Decrypt_InvalidIV()
		{
			byte[] key = SecureRandomGenerator.GetBytes(32);
			using var se = new RatchetCrypter(key);

			byte[] cipherblob = SecureRandomGenerator.GetBytes(100);

			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Decrypt(cipherblob, new byte[0])
			);
			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Decrypt(cipherblob, new byte[RatchetCrypter.IV_SIZE - 1])
			);
			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Decrypt(cipherblob, new byte[RatchetCrypter.IV_SIZE + 1])
			);
			Assert.ThrowsException<CryptoParameterException>(() =>
				se.Decrypt(cipherblob, new byte[RatchetCrypter.IV_SIZE * 2])
			);
		}

		[TestMethod]
		public void Decrypt_InvalidCipherblobLength()
		{
			byte[] key = SecureRandomGenerator.GetBytes(32);
			using var se = new RatchetCrypter(key);

			byte[] iv = SecureRandomGenerator.GetBytes(16);

			Assert.ThrowsException<FormatException>(() =>
				se.Decrypt(new byte[RatchetCrypter.IV_SIZE - 1], iv)
			);
			Assert.ThrowsException<FormatException>(() =>
				se.Decrypt(new byte[RatchetCrypter.IV_SIZE + 1], iv)
			);
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("S")]
		[DataRow("Medium length")]
		[DataRow("Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test!")]
		[DataRow("Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test! Long text to encrypt so that multiple blocks are used in CBC mode - this is an important test!")]
		public void RoundTrip_Text(string plaintext)
		{
			byte[] key = SecureRandomGenerator.GetBytes(32);
			using var se = new RatchetCrypter(key);

			byte[] plainblob = Encoding.UTF8.GetBytes(plaintext);

			byte[] package = se.Package(plainblob);
			byte[] newPlainblob = se.Unpackage(package);

			string newPlaintext = Encoding.UTF8.GetString(newPlainblob);

			Assert.AreEqual(plaintext, newPlaintext);
		}

		[TestMethod]
		public void RoundTrip_Empty()
		{
			byte[] key = SecureRandomGenerator.GetBytes(32);
			using var se = new RatchetCrypter(key);

			byte[] cipherblob = se.Package(new byte[0]);
			byte[] newPlainblob = se.Unpackage(cipherblob);

			CollectionAssert.AreEqual(newPlainblob, new byte[0]);
		}
	}
	*/
}