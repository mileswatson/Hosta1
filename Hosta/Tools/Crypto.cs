using System.IO;
using System.Security.Cryptography;

namespace Hosta.Tools
{
	/// <summary>
	/// A collection of cryptographic tools.
	/// </summary>
	public static class Crypto
	{
		/// <summary>
		/// Size of the AES key in bytes.
		/// </summary>
		public const int SYMMETRIC_KEY_SIZE = 32;

		/// <summary>
		/// Size of the AES IV in bytes.
		/// </summary>
		public const int SYMMETRIC_IV_SIZE = 16;

		/// <summary>
		/// Size of the HASH/HMAC in bytes.
		/// </summary>
		public const int HASH_SIZE = 32;

		/// <summary>
		/// Generates cryptographically-secure random bytes.
		/// </summary>
		/// <param name="size">The number of bytes.</param>
		/// <returns>The generated random bytes.</returns>
		public static byte[] SecureRandomBytes(int size)
		{
			var rng = RandomNumberGenerator.Create();
			byte[] randombytes = new byte[size];
			rng.GetBytes(randombytes);
			return randombytes;
		}

		/// <summary>
		/// Hashes data.
		/// </summary>
		/// <param name="data">Data to hash.</param>
		/// <returns>The hash of the data.</returns>
		public static byte[] Hash(byte[] data)
		{
			using var hasher = SHA256.Create();
			return hasher.ComputeHash(data);
		}

		/// <summary>
		/// Calculates an HMAC.
		/// </summary>
		/// <param name="data">The data to calculate HMAC from.</param>
		/// <param name="key">The key to calculate HMAC from.</param>
		/// <returns>The HMAC of the data and key.</returns>
		public static byte[] HMAC(byte[] data, byte[] key)
		{
			using var hmac = new HMACSHA256(key);
			return hmac.ComputeHash(data);
		}

		/// <summary>
		/// Encrypts data using AES256 in CBC mode.
		/// </summary>
		/// <param name="plainblob">The plainblob to encrypt.</param>
		/// <param name="key">The key to use.</param>
		/// <param name="iv">The IV to use.</param>
		/// <returns></returns>
		public static byte[] Encrypt(byte[] plainblob, byte[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;

			using MemoryStream cipherstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(cipherstream, aes.CreateEncryptor(), CryptoStreamMode.Write);
			cryptostream.Write(plainblob, 0, plainblob.Length);
			cryptostream.Close();
			return cipherstream.ToArray();
		}

		/// <summary>
		/// Decrypts data using AES256 in CBC mode.
		/// </summary>
		/// <param name="cipherblob">The cipherblob to decrypt.</param>
		/// <param name="key">The key to use.</param>
		/// <param name="iv">The IV to use.</param>
		/// <returns>The decrypted plainblob.</returns>
		public static byte[] Decrypt(byte[] cipherblob, byte[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;

			using MemoryStream plainstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(plainstream, aes.CreateDecryptor(), CryptoStreamMode.Write);
			cryptostream.Write(cipherblob, 0, cipherblob.Length);
			cryptostream.Close();
			return plainstream.ToArray();
		}
	}
}