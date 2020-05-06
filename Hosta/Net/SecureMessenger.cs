using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Hosta.Exceptions;
using Hosta.Tools;

namespace Hosta.Net
{
	public class SecureMessenger : IConversable
	{
		readonly IConversable rawMessenger;
		byte[] sharedKey;

		// to ensure attackers cannot spam duplicate queries
		readonly HashSet<byte[]> usedHMACs = new HashSet<byte[]>(new ByteArrayComparer());

		public SecureMessenger(IConversable rawMessenger)
		{
			this.rawMessenger = rawMessenger;
		}

		public async Task Establish()
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
		public Task Send(byte[] data)
		{
			byte[] secureMessage = ConstructSecurePackage(data);
			return rawMessenger.Send(secureMessage);
		}

		public async Task<byte[]> Receive()
		{
			byte[] package = await rawMessenger.Receive();
			return OpenSecurePackage(package);
		}

		/// <summary>
		/// Securely receives a message.
		/// </summary>
		/// <returns>An awaitable task that resolves to the decrypted message.</returns>
		

		/// <summary>
		/// Message will be constructed in the format:
		/// 16B IV, ciphertext, 32B HMAC
		/// </summary>
		/// <param name="plainblob">The data to secure.</param>
		/// <returns>The secure message package.</returns>
		byte[] ConstructSecurePackage(byte[] plainblob)
		{
			// Generate an IV
			byte[] head = Crypto.SecureRandomBytes(Crypto.SYMMETRIC_IV_SIZE);

			// Encrypt the plaintext
			byte[] body = Crypto.Encrypt(plainblob, sharedKey, head);

			// Prepend the IV to the ciphertext
			byte[] headAndBody = new byte[head.Length + body.Length];
			Array.Copy(head, 0, headAndBody, 0, head.Length);
			Array.Copy(body, 0, headAndBody, head.Length, body.Length);

			// Calculate the HMAC of the first two parts
			byte[] tail = Crypto.HMAC(headAndBody, sharedKey);

			// Construct the final package
			byte[] package = new byte[headAndBody.Length + tail.Length];
			Array.Copy(headAndBody, 0, package, 0, headAndBody.Length);
			Array.Copy(tail, 0, package, headAndBody.Length, tail.Length);

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

			// Separate the tail from the rest of the package
			byte[] headAndBody = new byte[package.Length - Crypto.HASH_SIZE];
			Array.Copy(package, 0, headAndBody, 0, headAndBody.Length);
				
			byte[] tail = new byte[Crypto.HASH_SIZE];
			Array.Copy(package, headAndBody.Length, tail, 0, tail.Length);

			// Check that the HMAC has not been used before
			if (usedHMACs.Contains(tail))
			{
				throw new DuplicatePackageException("Duplicate HMAC received.");
			}

			// Verify the integrity of the message
			byte[] actualHMAC = Crypto.HMAC(headAndBody, sharedKey);
			if (!tail.SequenceEqual(actualHMAC))
			{
				throw new TamperedPackageException("HMAC does not match received package.");
			}

			// Separate the IV and ciphertext
			byte[] head = new byte[Crypto.SYMMETRIC_IV_SIZE];
			Array.Copy(headAndBody, 0, head, 0, head.Length);

			byte[] body = new byte[headAndBody.Length - Crypto.SYMMETRIC_IV_SIZE];
			Array.Copy(headAndBody, head.Length, body, 0, body.Length);

			// Decrypt the ciphertext
			byte[] plainblob = Crypto.Decrypt(body, sharedKey, head);

			return plainblob;
		}

		class ByteArrayComparer : IEqualityComparer<byte[]>
		{
			public bool Equals(byte[] a, byte[] b)
			{
				if (a.Length != b.Length) return false;
				for (int i = 0; i < a.Length; i++)
					if (a[i] != b[i]) return false;
				return true;
			}
			public int GetHashCode(byte[] data)
			{
				return BitConverter.ToInt32(data, 0);
			}
		}

	}
}
