using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

using Hosta.Exceptions;
using Hosta.Tools;

namespace Hosta.Crypto
{
	/// <summary>
	/// Used to create and open AES-encrypted verifiable packages.
	/// </summary>
	public class RatchetCrypter
	{
		public const int KEY_SIZE = 32;
		public const int BLOCK_SIZE = 16;
		public const int NONCE_SIZE = 12;

		private readonly HashRatchet encryptRatchet = new HashRatchet();
		private readonly HashRatchet decryptRatchet = new HashRatchet();

		/// <summary>
		/// Sets the number of clicks to turn on the encrypt ratchet.
		/// </summary>
		public byte[] EncryptClicks {
			set {
				encryptRatchet.Clicks = value;
			}
		}

		/// <summary>
		/// Sets the number of clicks to turn on the decrypt ratchet.
		/// </summary>
		public byte[] DecryptClicks {
			set {
				decryptRatchet.Clicks = value;
			}
		}

		/// <summary>
		/// Creates a new AesEncryptor.
		/// </summary>
		/// <param name="encryptKey">The starting encrypt key.</param>
		/// <param name="encryptKey">The starting decrypt key.</param>
		public RatchetCrypter(byte[] encryptKey, byte[] decryptKey)
		{
			encryptRatchet.Clicks = encryptKey;
			decryptRatchet.Clicks = decryptKey;
		}

		/// <summary>
		/// Encrypts data using AES-GCM (includes MAC).
		/// </summary>
		/// <param name="plainblob">The plainblob to encrypt.</param>
		/// <returns>The encrypted package.</returns>
		public byte[] Encrypt(byte[] plainblob)
		{
			// Generate the new encryption key
			encryptRatchet.Turn();
			byte[] key = encryptRatchet.Output;

			// Generate the random nonce
			byte[] nonce = SecureRandomGenerator.GetBytes(NONCE_SIZE);

			// Create the empty partitions to be filled
			byte[] cipherblob = new byte[plainblob.Length];
			byte[] tag = new byte[16];

			// Encrypt the data, fill the cipherblob and tag
			using var aes = new AesGcm(key);
			aes.Encrypt(nonce, plainblob, cipherblob, tag);

			// Combine partitions into a package and return
			return Blobs.Combine(tag, nonce, cipherblob);
		}

		/// <summary>
		/// Decrypts data using AES-GCM (and verifies the MAC).
		/// </summary>
		/// <param name="package">The encrypted package to deconstruct and decrypt.</param>
		/// <exception cref="InvalidPackageException"/>
		/// <returns>The decrypted plainblob.</returns>
		public byte[] Decrypt(byte[] package)
		{
			// Check that package has a valid length
			if (package.Length - NONCE_SIZE - BLOCK_SIZE < 0) throw new InvalidPackageException("The package size was invalid!");

			// Generate the new decrypt key
			decryptRatchet.Turn();
			byte[] key = decryptRatchet.Output;

			// Create empty space for the partitions
			byte[] tag = new byte[BLOCK_SIZE];
			byte[] nonce = new byte[NONCE_SIZE];
			byte[] cipherblob = new byte[package.Length - BLOCK_SIZE - NONCE_SIZE];
			byte[] plainblob = new byte[cipherblob.Length];

			// Populate the partitions
			Blobs.Split(package, tag, nonce, cipherblob);

			// Decrypt the cipherblob
			try
			{
				using var aes = new AesGcm(key);
				aes.Decrypt(nonce, cipherblob, tag, plainblob);
			}
			catch
			{
				throw new InvalidPackageException("The message could not be decrypted!");
			}

			return plainblob;
		}
	}
}