using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

using Hosta.Exceptions;

namespace Hosta.Net
{
	internal class SecureMessenger
	{
		RawMessenger rawMessenger;
		byte[] sharedKey;
		HashSet<byte[]> usedIVs;	// to ensure attackers cannot spam duplicate queries

		public SecureMessenger(RawMessenger rawMessenger)
		{
			this.rawMessenger = rawMessenger;
		}

		public async Task Secure()
		{
			// Generates the local private key
			ECDiffieHellmanCng privateKey = new ECDiffieHellmanCng(521);
			var sent = rawMessenger.Send(privateKey.PublicKey.ToByteArray());

			// Reassembles the other users 
			CngKey foreignPublicKey = CngKey.Import(await rawMessenger.Receive(), CngKeyBlobFormat.EccPublicBlob);
			
			// Uses sha256 by default
			sharedKey = privateKey.DeriveKeyMaterial(foreignPublicKey);

			// Ensures the other user has received the message
			await sent;
		}

		public async Task Send(string data)
		{
			byte[] plainblob = Encoding.UTF8.GetBytes(data);
			byte[] secureMessage = ConstructSecureMessage(plainblob);
			await rawMessenger.Send(secureMessage);
		}

		public async Task<string> Receive()
		{
			byte[] combined = await rawMessenger.Receive();
			using (AesCng aes = new AesCng())
			{
				aes.Key = sharedKey;
				Array.Copy(combined, 0, aes.IV, 0, 16);

				byte[] cipherblob = new byte[combined.Length - 16];
				Array.Copy(combined, 16, cipherblob, 0, cipherblob.Length);

				using (MemoryStream plainstream = new MemoryStream())
				using (CryptoStream cryptostream = new CryptoStream(plainstream, aes.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cryptostream.Write(cipherblob, 0, cipherblob.Length);
					cryptostream.Close();
					return Encoding.UTF8.GetString(plainstream.ToArray());
				}
			}
		}

		/// <summary>
		/// Message will be constructed in the format:
		/// 16B IV, ciphertext, 32B HMAC
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <returns>The secure message package.</returns>
		byte[] ConstructSecureMessage(byte[] plainblob)
		{
			byte[] package;
			using (AesCng aes = new AesCng())
			{
				aes.Key = sharedKey;
				while (usedIVs.Contains(aes.IV))
				{
					aes.GenerateIV();
				}
				using (MemoryStream cipherstream = new MemoryStream())
				using (CryptoStream encryptor = new CryptoStream(cipherstream,
					aes.CreateEncryptor(), CryptoStreamMode.Write))
				{
					encryptor.Write(plainblob, 0, plainblob.Length);
					encryptor.Close();
					package = new byte[aes.IV.Length + cipherstream.Length + 32];

					usedIVs.Add(aes.IV);

					Array.Copy(aes.IV, 0, package, 0, aes.IV.Length);
					Array.Copy(cipherstream.ToArray(), 0,
						package, aes.IV.Length,
						cipherstream.Length);
				}
			}
			using (var hmac = new HMACSHA256(sharedKey))
			{
				Array.Copy(
					hmac.ComputeHash(package, 0, package.Length-32), 0,
					package, package.Length-32,
					32);
			}
			return package;
		}

		/// <summary>
		/// Deconstruct secure message, raises exceptions if unable to authenticate.
		/// </summary>
		/// <param name="combined"></param>
		/// <returns></returns>
		byte[] DeconstructSecureMessage(byte[] package)
		{

			using (var hmac = new HMACSHA256(sharedKey))
			{

			}

			using (var aes = new AesCng())
			{
				aes.Key = sharedKey;
				Array.Copy(package, 0, aes.IV, 0, aes.IV.Length);

				if (usedIVs.Contains(aes.IV))
				{
					throw new TamperedMessageException();
				}

				byte[] cipherblob = new byte[package.Length - aes.IV.Length - 32];
				Array.Copy(package, aes.IV.Length, cipherblob, aes.IV.Length, cipherblob.Length);

				using (MemoryStream plainstream = new MemoryStream())
				using (CryptoStream cryptostream = new CryptoStream(plainstream,
					aes.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cryptostream.Write(cipherblob, 0, cipherblob.Length);
					cryptostream.Close();
					return plainstream.ToArray();
				}
			}
		}

	}
}
