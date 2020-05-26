using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hosta.Crypto
{
	/// <summary>
	/// Offers key ratcheting functionality.
	/// </summary>
	public class HashRatchet : IDisposable
	{
		private byte[] secret;

		private byte[] clicks;

		public byte[] Clicks {
			set {
				ThrowIfDisposed();
				clicks = value;
			}
		}

		public byte[] Output {
			get {
				ThrowIfDisposed();
				return Hasher.HMAC(new byte[32], secret);
			}
		}

		public HashRatchet(byte[] clicks = null)
		{
			if (clicks == null) clicks = new byte[Hasher.OUTPUT_SIZE];
			this.secret = clicks;
		}

		public void Turn()
		{
			ThrowIfDisposed();
			secret = Hasher.HMAC(secret, clicks);
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("HMACRatchet has been disposed!");
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
				if (secret != null)
				{
					for (int i = 0; i < secret.Length; i++)
					{
						secret[i] = 0;
					}
				}
				if (clicks != null)
				{
					for (int i = 0; i < clicks.Length; i++)
					{
						clicks[i] = 0;
					}
				}
			}

			disposed = true;
		}
	}
}