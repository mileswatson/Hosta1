using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

using Hosta.Exceptions;

namespace Hosta.Crypto
{
	/// <summary>
	/// Used to create and open AES-encrypted verifiable packages.
	/// </summary>
	public class RatchetCrypter : IDisposable
	{
		public const int KEY_SIZE = 32;
		public const int BLOCK_SIZE = 16;
		public const int NONCE_SIZE = 12;

		private readonly HashRatchet encryptRatchet = new HashRatchet();
		private readonly HashRatchet decryptRatchet = new HashRatchet();

		public byte[] EncryptClicks {
			set {
				encryptRatchet.Clicks = value;
			}
		}

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
		/// <exception cref="CryptoParameterException" />
		/// <returns></returns>
		public byte[] Encrypt(byte[] plainblob)
		{
			ThrowIfDisposed();

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
			return Combine(tag, nonce, cipherblob);
		}

		/// <summary>
		/// Decrypts data using AES-GCM (and verifies the MAC).
		/// </summary>
		/// <param name="cipherblob">The cipherblob to decrypt.</param>
		/// <exception cref="CryptoParameterException" />
		/// <exception cref="FormatException" />
		/// <returns>The decrypted plainblob.</returns>
		public byte[] Decrypt(byte[] package)
		{
			ThrowIfDisposed();

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
			Split(package, tag, nonce, cipherblob);

			// Decrypt the cipherblob
			using var aes = new AesGcm(key);
			aes.Decrypt(nonce, cipherblob, tag, plainblob);

			return plainblob;
		}

		public static byte[] Combine(params byte[][] sources)
		{
			List<byte> destination = new List<byte>();
			foreach (byte[] blob in sources) destination.AddRange(blob);
			return destination.ToArray();
		}

		public static void Split(byte[] source, params byte[][] destinations)
		{
			// Check that the combined destinations are as big as the source
			int total = 0;
			foreach (byte[] destination in destinations) total += destination.Length;
			if (total != source.Length) throw new InvalidPackageException("Package cannot be split into the correct sized parts.");

			// Copy each part over
			int index = 0;
			foreach (byte[] destination in destinations)
			{
				Array.Copy(source, index, destination, 0, destination.Length);
				index += destination.Length;
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("SymmetricEncryptor has been disposed!");
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Disposed of managed resources
				if (encryptRatchet != null) encryptRatchet.Dispose();
				if (decryptRatchet != null) decryptRatchet.Dispose();
			}

			disposed = true;
		}
	}
}