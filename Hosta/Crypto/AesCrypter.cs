using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Hosta.Exceptions;

namespace Hosta.Crypto
{
	/// <summary>
	/// Used to create and open AES-encrypted verifiable packages.
	/// </summary>
	public class AesCrypter : IDisposable
	{
		/// <summary>
		/// Internal key used to encrypt and decrypt.
		/// </summary>
		private byte[] key;

		public const int KEY_SIZE = 32;

		public const int IV_SIZE = 16;

		/// <summary>
		/// Sets the key, if in the correct format.
		/// </summary>
		public byte[] Key {
			set {
				ThrowIfDisposed();
				if (value.Length != KEY_SIZE)
				{
					throw new CryptoParameterException("Key was not the correct length!");
				}
				key = value;
			}
		}

		/// <summary>
		/// Creates a new AesEncryptor.
		/// </summary>
		/// <param name="key">The key used to encrypt and decrypt data.</param>
		public AesCrypter(byte[] key = null)
		{
			if (key != null) Key = key;
			else Key = SecureRandomGenerator.GetBytes(KEY_SIZE);
		}

		/// <summary>
		/// Constructs a secure package.
		/// </summary>
		/// <param name="plainblob">The data to secure.</param>
		/// <returns></returns>
		public byte[] Package(byte[] plainblob)
		{
			// Generate an IV
			byte[] head = SecureRandomGenerator.GetBytes(IV_SIZE);

			// Encrypt the plaintext
			byte[] body = Encrypt(plainblob, head);

			// Prepend the IV to the ciphertext
			byte[] headAndBody = new byte[head.Length + body.Length];
			Array.Copy(head, 0, headAndBody, 0, head.Length);
			Array.Copy(body, 0, headAndBody, head.Length, body.Length);

			// Calculate the HMAC of the first two parts
			byte[] tail = Hasher.HMAC(headAndBody, key);

			// Construct the final package
			byte[] package = new byte[headAndBody.Length + tail.Length];
			Array.Copy(headAndBody, 0, package, 0, headAndBody.Length);
			Array.Copy(tail, 0, package, headAndBody.Length, tail.Length);

			return package;
		}

		/// <summary>
		/// Pads data using PKCS7, then encrypts using AES256 in CBC mode.
		/// </summary>
		/// <param name="plainblob">The plainblob to encrypt.</param>
		/// <param name="iv">The IV to use.</param>
		/// <exception cref="CryptoParameterException" />
		/// <returns></returns>
		public byte[] Encrypt(byte[] plainblob, byte[] iv)
		{
			ThrowIfDisposed();

			// Checks that the iv is of the correct length.
			if (iv.Length != IV_SIZE)
			{
				throw new CryptoParameterException("IV is not the correct size!");
			}

			using var aes = new AesManaged()
			{
				Key = key,
				IV = iv,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7
			};

			using MemoryStream cipherstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(cipherstream, aes.CreateEncryptor(), CryptoStreamMode.Write);
			cryptostream.Write(plainblob, 0, plainblob.Length);
			cryptostream.Close();

			return cipherstream.ToArray();
		}

		/// <summary>
		/// Unpackages a secure package.
		/// </summary>
		/// <param name="package"></param>
		/// <returns></returns>
		public byte[] Unpackage(byte[] package)
		{
			// Separate the tail from the rest of the package
			byte[] headAndBody = new byte[package.Length - Hasher.OUTPUT_SIZE];
			Array.Copy(package, 0, headAndBody, 0, headAndBody.Length);

			byte[] tail = new byte[Hasher.OUTPUT_SIZE];
			Array.Copy(package, headAndBody.Length, tail, 0, tail.Length);

			// Verify the integrity of the message
			byte[] actualHMAC = Hasher.HMAC(headAndBody, key);
			if (!tail.SequenceEqual(actualHMAC))
			{
				throw new TamperedPackageException("HMAC does not match received package.");
			}

			// Separate the IV and ciphertext
			byte[] head = new byte[IV_SIZE];
			Array.Copy(headAndBody, 0, head, 0, head.Length);

			byte[] body = new byte[headAndBody.Length - IV_SIZE];
			Array.Copy(headAndBody, head.Length, body, 0, body.Length);

			// Decrypt the cipher-text
			byte[] plainblob = Decrypt(body, head);

			return plainblob;
		}

		/// <summary>
		/// Decrypts data using AES256 in CBC mode,
		/// and strips it of PKCS7 padding.
		/// </summary>
		/// <param name="cipherblob">The cipherblob to decrypt.</param>
		/// <param name="iv">The IV to use.</param>
		/// <exception cref="CryptoParameterException" />
		/// <exception cref="FormatException" />
		/// <returns>The decrypted plainblob.</returns>
		public byte[] Decrypt(byte[] cipherblob, byte[] iv)
		{
			ThrowIfDisposed();

			// Check that the IV has the correct length
			if (iv.Length != IV_SIZE)
			{
				throw new CryptoParameterException("IV does not have the correct size.");
			}

			// Check that the cipherblob length is a multiple of the block size in bytes
			if (cipherblob.Length % IV_SIZE != 0)
			{
				throw new FormatException("Cipherblob length is not a multiple of the IV size!");
			}

			using var aes = new AesManaged()
			{
				Key = key,
				IV = iv,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7
			};

			using MemoryStream plainstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(plainstream, aes.CreateDecryptor(), CryptoStreamMode.Write);
			cryptostream.Write(cipherblob, 0, cipherblob.Length);
			cryptostream.Close();
			return plainstream.ToArray();
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
				if (key != null)
				{
					for (int i = 0; i < key.Length; i++)
					{
						key[i] = 0;
					}
				}
			}

			disposed = true;
		}
	}
}