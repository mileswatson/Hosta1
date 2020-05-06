using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hosta.Tools
{
	public static class Crypto
	{

		public const int SYMMETRIC_KEY_SIZE = 32;
		public const int SYMMETRIC_IV_SIZE = 16;

		public const int HASH_SIZE = 32;

		public static byte[] SecureRandomBytes(int size)
		{
			var rng = RandomNumberGenerator.Create();
			byte[] randombytes = new byte[size];
			rng.GetBytes(randombytes);
			return randombytes;
		}

		public static byte[] Hash(byte[] data)
		{
			using var hasher = SHA256.Create();
			return hasher.ComputeHash(data);
		}

		public static byte[] HMAC(byte[] data, byte[] key)
		{
			using var hmac = new HMACSHA256(key);
			return hmac.ComputeHash(data);
		}

		public static byte[] Encrypt(byte[] plainblob, byte[] key, byte[] iv)
		{
			using AesCng aes = new AesCng
			{
				Key = key,
				IV = iv
			};

			using MemoryStream cipherstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(cipherstream, aes.CreateEncryptor(), CryptoStreamMode.Write);
			cryptostream.Write(plainblob, 0, plainblob.Length);
			cryptostream.Close();
			return cipherstream.ToArray();
		}

		public static byte[] Decrypt(byte[] cipherblob, byte[] key, byte[] iv)
		{
			using AesCng aes = new AesCng
			{
				Key = key,
				IV = iv
			};
			using MemoryStream plainstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(plainstream, aes.CreateDecryptor(), CryptoStreamMode.Write);
			cryptostream.Write(cipherblob, 0, cipherblob.Length);
			cryptostream.Close();
			return plainstream.ToArray();
		}
	}
}
