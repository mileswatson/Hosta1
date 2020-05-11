using Hosta.Crypto;
using Hosta.Exceptions;
using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to establish and send messages over an encrypted session.
	/// </summary>
	public class SecureConversation : IConversable
	{
		/// <summary>
		/// The insecure conversation to talk over.
		/// </summary>
		private readonly IConversable insecureConversation;

		/// <summary>
		/// The shared AES and HMAC key.
		/// </summary>
		private SymmetricEncryptor cryptor;

		/// <summary>
		/// A custom HashSet to keep a record of which valid
		/// HMACs have already been used.
		/// </summary>
		private readonly HashSet<byte[]> usedHMACs = new HashSet<byte[]>(new HmacComparer());

		/// <summary>
		/// Constructs a new SecureConversation over an insecure conversation.
		/// </summary>
		/// <param name="insecureConversation">
		/// The insecure conversation to talk over.
		/// </param>
		public SecureConversation(IConversable insecureConversation)
		{
			this.insecureConversation = insecureConversation;
		}

		/// <summary>
		/// Performs a key exchange across the insecure connection.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		public async Task Establish()
		{
			// Generates the local private key
			ECDiffieHellman privateKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);

			// Sends the local part
			byte[] localToken = privateKey.ExportSubjectPublicKeyInfo();
			var sent = insecureConversation.Send(localToken);

			// Receives the foreign part
			byte[] foreignToken = await insecureConversation.Receive();
			ECDiffieHellman foreignPublicKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
			foreignPublicKey.ImportSubjectPublicKeyInfo(foreignToken, out _);

			// Derives key using SHA256 by default.
			byte[] sharedKey = privateKey.DeriveKeyFromHash(foreignPublicKey.PublicKey, HashAlgorithmName.SHA256);
			cryptor = new SymmetricEncryptor(sharedKey);

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
			ThrowIfDisposed();
			byte[] secureMessage = ConstructSecurePackage(data);
			return insecureConversation.Send(secureMessage);
		}

		/// <summary>
		/// Asynchronously receives an encrypted message.
		/// </summary>
		/// <returns>
		/// An awaitable task that resolves to the decrypted message.
		/// </returns>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();
			byte[] package = await insecureConversation.Receive();
			return OpenSecurePackage(package);
		}

		/// <summary>
		/// Packages IV, ciphertext, and HMAC together.
		/// </summary>
		/// <param name="plainblob">The data to secure.</param>
		/// <returns>The secure message package.</returns>
		private byte[] ConstructSecurePackage(byte[] plainblob)
		{
			// Generate an IV
			byte[] head = SecureRandomGenerator.GetBytes(SymmetricEncryptor.IV_SIZE);

			// Encrypt the plaintext
			byte[] body = cryptor.Encrypt(plainblob, head);

			// Prepend the IV to the ciphertext
			byte[] headAndBody = new byte[head.Length + body.Length];
			Array.Copy(head, 0, headAndBody, 0, head.Length);
			Array.Copy(body, 0, headAndBody, head.Length, body.Length);

			// Calculate the HMAC of the first two parts
			byte[] tail = Hasher.HMAC(headAndBody, cryptor.KeyHash);

			// Construct the final package
			byte[] package = new byte[headAndBody.Length + tail.Length];
			Array.Copy(headAndBody, 0, package, 0, headAndBody.Length);
			Array.Copy(tail, 0, package, headAndBody.Length, tail.Length);

			return package;
		}

		/// <summary>
		/// Verifies HMAC and decrypts message.
		/// </summary>
		/// <param name="package">The secure message package.</param>
		/// <exception cref="DuplicatePackageException"/>
		/// <exception cref="TamperedPackageException"/>
		/// <returns>The package contents.</returns>
		private byte[] OpenSecurePackage(byte[] package)
		{
			// Separate the tail from the rest of the package
			byte[] headAndBody = new byte[package.Length - Hasher.OUTPUT_SIZE];
			Array.Copy(package, 0, headAndBody, 0, headAndBody.Length);

			byte[] tail = new byte[Hasher.OUTPUT_SIZE];
			Array.Copy(package, headAndBody.Length, tail, 0, tail.Length);

			// Check that the HMAC has not been used before
			if (usedHMACs.Contains(tail))
			{
				throw new DuplicatePackageException("Duplicate HMAC received.");
			}

			// Verify the integrity of the message
			byte[] actualHMAC = Hasher.HMAC(headAndBody, cryptor.KeyHash);
			if (!tail.SequenceEqual(actualHMAC))
			{
				throw new TamperedPackageException("HMAC does not match received package.");
			}

			// Separate the IV and ciphertext
			byte[] head = new byte[SymmetricEncryptor.IV_SIZE];
			Array.Copy(headAndBody, 0, head, 0, head.Length);

			byte[] body = new byte[headAndBody.Length - SymmetricEncryptor.IV_SIZE];
			Array.Copy(headAndBody, head.Length, body, 0, body.Length);

			// Decrypt the cipher-text
			byte[] plainblob = cryptor.Decrypt(body, head);

			return plainblob;
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("SecureConversation has been disposed!");
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
				// Dispose of managed resources
				if (insecureConversation != null) insecureConversation.Dispose();
			}

			disposed = true;
		}

		/// <summary>
		/// Used by the HashSet to prevent duplicate HMACs.
		/// </summary>
		private class HmacComparer : IEqualityComparer<byte[]>
		{
			/// <summary>
			/// Checks if two HMACs are the same.
			/// </summary>
			/// <param name="a">HMAC to compare.</param>
			/// <param name="b">HMAC to compare.</param>
			/// <returns>True if they are the same, otherwise false.</returns>
			public bool Equals(byte[] a, byte[] b)
			{
				if (a.Length != b.Length) return false;
				for (int i = 0; i < a.Length; i++)
					if (a[i] != b[i]) return false;
				return true;
			}

			/// <summary>
			/// Returns the first 4 bytes of the HMAC.
			/// </summary>
			/// <param name="data">HMAC to get the hash-code of.</param>
			/// <returns></returns>
			public int GetHashCode(byte[] data)
			{
				return BitConverter.ToInt32(Hasher.Hash(data), 0);
			}
		}
	}
}