using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Hosta.Crypto
{
	public class ECDHRatchet : IDisposable
	{
		private ECDiffieHellman secret;

		private byte[] output;

		public byte[] Output {
			get {
				return output.Clone() as byte[];
			}
		}

		public const int OUTPUT_SIZE = 64;

		/// <summary>
		/// Exports the local token used by the ratchet.
		/// </summary>
		public Token PublicToken {
			get {
				return new Token(secret.ExportSubjectPublicKeyInfo());
			}
		}

		/// <summary>
		/// Initialises a new instance of ECDHRatchet.
		/// </summary>
		public ECDHRatchet()
		{
			secret = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
		}

		/// <summary>
		/// Generates a new ratchet without compromising the output.
		/// </summary>
		public void New()
		{
			ThrowIfDisposed();
			secret = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
		}

		/// <summary>
		/// Derives the output from a foreign token.
		/// </summary>
		/// <param name="t">The foreign token.</param>
		public void Turn(Token t)
		{
			ThrowIfDisposed();
			output = secret.DeriveKeyFromHash(t.Value, HashAlgorithmName.SHA512);
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("DiffieHellmanRatchet has been disposed!");
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
				if (secret != null) secret.Dispose();
			}

			disposed = true;
		}

		/// <summary>
		/// To represent an ECDH public key.
		/// </summary>
		public class Token : IDisposable
		{
			/// <summary>
			/// The internal foreign public key to export to/from.
			/// </summary>
			private readonly ECDiffieHellman foreignPublicKey =
				ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

			/// <summary>
			/// Returns the internal public key.
			/// </summary>
			public ECDiffieHellmanPublicKey Value {
				get {
					ThrowIfDisposed();
					return foreignPublicKey.PublicKey;
				}
			}

			/// <summary>
			/// Constructs a new Token from exported bytes.
			/// </summary>
			/// <param name="exportedKey"></param>
			public Token(byte[] exportedKey)
			{
				try
				{
					foreignPublicKey.ImportSubjectPublicKeyInfo(exportedKey, out _);
				}
				catch (Exception e)
				{
					Dispose();
					throw e;
				}
			}

			/// <summary>
			/// Exports the public key as bytes.
			/// </summary>
			/// <returns></returns>
			public byte[] GetBytes()
			{
				return foreignPublicKey.ExportSubjectPublicKeyInfo();
			}

			/// <summary>
			/// Checks if a Token is different to another Token.
			/// </summary>
			/// <param name="t"></param>
			/// <returns></returns>
			public bool IsDifferentTo(Token t)
			{
				var a = GetBytes();
				var b = t.GetBytes();
				if (a.Length != b.Length) return true;
				for (int i = 0; i < a.Length; i++)
				{
					if (a[i] != b[i]) return true;
				}
				return false;
			}

			//// Implements IDisposable

			private bool disposed = false;

			private void ThrowIfDisposed()
			{
				if (disposed) throw new ObjectDisposedException("ECDHRatchet.Token has been disposed!");
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
					if (foreignPublicKey != null) foreignPublicKey.Dispose();
				}

				disposed = true;
			}
		}
	}
}