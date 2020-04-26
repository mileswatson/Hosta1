using Hosta.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hosta.Net
{
	internal class SecureMessenger
	{
		readonly RawMessenger rawMessenger;
		byte[] sharedKey;

		// to ensure attackers cannot spam duplicate queries
		readonly HashSet<byte[]> usedIVs = new HashSet<byte[]>();

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

		/// <summary>
		/// Asynchronously sends an encrypted message.
		/// </summary>
		/// <param name="data">The message to encrypt and send.</param>
		/// <returns>An awaitable task.</returns>
		public async Task Send(string data)
		{
			byte[] plainblob = Encoding.UTF8.GetBytes(data);
			byte[] secureMessage = ConstructSecurePackage(plainblob);
			await rawMessenger.Send(secureMessage);
		}

		/// <summary>
		/// Securely receives a message.
		/// </summary>
		/// <returns>An awaitable task that resolves to the decrypted message.</returns>
		public async Task<string> Receive()
		{
			byte[] package = await rawMessenger.Receive();
			return Encoding.UTF8.GetString(OpenSecurePackage(package));
		}

		/// <summary>
		/// Message will be constructed in the format:
		/// 16B IV, ciphertext, 32B HMAC
		/// </summary>
		/// <param name="plainblob">The data to secure.</param>
		/// <returns>The secure message package.</returns>
		byte[] ConstructSecurePackage(byte[] plainblob)
		{
			byte[] package;
			using (AesCng aes = new AesCng())
			{
				aes.Key = sharedKey;
				while (usedIVs.Contains(aes.IV))
				{
					aes.GenerateIV();
				}
				using MemoryStream cipherstream = new MemoryStream();
				using CryptoStream encryptor = new CryptoStream(cipherstream,
					aes.CreateEncryptor(), CryptoStreamMode.Write);
				encryptor.Write(plainblob, 0, plainblob.Length);
				encryptor.Close();
				package = new byte[16 + cipherstream.Length + 32];

				usedIVs.Add(aes.IV);

				Array.Copy(aes.IV, 0, package, 0, 16);
				Array.Copy(cipherstream.ToArray(), 0,
					package, 16,
					cipherstream.Length);
			}
			using (var hmac = new HMACSHA256(sharedKey))
			{
				Array.Copy(
					hmac.ComputeHash(package, 0, package.Length - 32), 0,
					package, package.Length - 32,
					32);
			}
			return package;
		}

		/// <summary>
		/// Deconstruct secure message, raises exceptions if unable to authenticate.
		/// </summary>
		/// <param name="package">The secure message package.</param>
		/// <exception cref="DuplicatePackageException"/>
		/// <exception cref="TamperedPackageException"/>
		/// <returns>The package contents.</returns>
		byte[] OpenSecurePackage(byte[] package)
		{
			byte[] IV = new byte[16];
			Array.Copy(package, 0, IV, 0, 16);

			if (usedIVs.Contains(IV))
			{
				throw new DuplicatePackageException("Duplicate IV received.");
			}

			using (var hmac = new HMACSHA256(sharedKey))
			{
				byte[] actualHMAC = hmac.ComputeHash(package, 0, package.Length - 32);
				for (int i = 0, j = package.Length - 32; i < 32; i++, j++)
				{
					if (actualHMAC[i] != package[j])
					{
						throw new TamperedPackageException("HMAC does not match received package.");
					}
				}
			}

			using var aes = new AesCng
			{
				Key = sharedKey,
				IV = IV
			};

			using MemoryStream plainstream = new MemoryStream();
			using CryptoStream cryptostream = new CryptoStream(plainstream,
				aes.CreateDecryptor(), CryptoStreamMode.Write);
			cryptostream.Write(package, 16, package.Length - 16 - 32);
			cryptostream.Close();
			return plainstream.ToArray();
		}

	}
}
