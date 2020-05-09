using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Hosta.Exceptions;

namespace Hosta.Crypto
{
	public class SymmetricEncryptor
	{
		/// <summary>
		/// Internal key used to encrypt and decrypt.
		/// </summary>
		readonly private byte[] key;

		/// <summary>
		/// Stores the hash of the key to be used for HMAC.
		/// </summary>
		readonly private byte[] keyHash;

		/// <summary>
		/// Returns the hash of the key to be used for HMAC.
		/// </summary>
		public byte[] KeyHash {
			get { return keyHash.Clone() as byte[]; }
			set { }
		}

		/// <summary>
		/// Size of the AES key in bytes.
		/// </summary>
		public const int KEY_SIZE = 32;

		/// <summary>
		/// Size of the AES IV in bytes.
		/// </summary>
		public const int IV_SIZE = 16;

		/// <summary>
		/// Creates a new SymmetricEncryptor.
		/// </summary>
		/// <param name="key">The key used to encrypt and decrypt data.</param>
		public SymmetricEncryptor(byte[] key)
		{
			if (key.Length != KEY_SIZE)
			{
				throw new CryptoParameterException("Key was not the correct length!");
			}
			this.key = key;
			keyHash = Hasher.Hash(key);
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
	}
}