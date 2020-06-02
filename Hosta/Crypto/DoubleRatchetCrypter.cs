namespace Hosta.Crypto
{
	/*
	/// <summary>
	/// Facilitates encrypting and decrypting using
	/// separate key ratchets for encryption and decryption.
	/// </summary>
	public class DoubleRatchetCrypter : IDisposable
	{
		private readonly HashRatchet encryptRatchet;
		private readonly HashRatchet decryptRatchet;

		private readonly ECDHRatchet secret;
		private ECDHRatchet.Token foreignToken;

		public byte[] LocalToken {
			get {
				ThrowIfDisposed();
				return secret.PublicToken.GetBytes();
			}
		}

		private enum State
		{
			Behind = -1,
			Equal = 0,
			Ahead = 1
		}

		private State state = State.Equal;

		private readonly bool isInitiator;

		/// <summary>
		/// Constructs a double ratchet crypter.
		/// </summary>
		/// <param name="initiated">
		/// Whether the underlying stream was initiated
		/// (rather than accepted).
		/// </param>
		public DoubleRatchetCrypter(bool initiated)
		{
			this.isInitiator = initiated;
			encryptRatchet = new HashRatchet(new byte[32]);
			decryptRatchet = new HashRatchet(new byte[32]);
			secret = new ECDHRatchet();
		}

		/// <summary>
		/// Performs the initial secret turn (ECDH).
		/// </summary>
		/// <param name="blob"></param>
		public void Establish(byte[] foreignTokenBlob)
		{
			ThrowIfDisposed();
			this.foreignToken = new ECDHRatchet.Token(foreignTokenBlob);
			secret.Turn(this.foreignToken);
			UpdateEncryptRatchetClicks();
			UpdateDecryptRatchetClicks();
		}

		/// <summary>
		/// Extract the encrypt part of the secret output, and
		/// updates the symmetric ratchet.
		/// </summary>
		private void UpdateEncryptRatchetClicks()
		{
			byte[] clicks = new byte[32];
			byte[] raw = secret.Output;
			int start = isInitiator ? 0 : 32;
			Array.Copy(raw, start, clicks, 0, 32);
			encryptRatchet.Clicks = clicks;
			//if (isInitiator) Console.WriteLine("Alice R CLICKS SET TO " + Transcoder.HexFromBytes(clicks));
			//else Console.WriteLine("Bob L CLICKS SET TO " + Transcoder.HexFromBytes(clicks));
		}

		/// <summary>
		/// Extract the decrypt part of the secret output, and
		/// updates the symmetric ratchet.
		/// </summary>
		private void UpdateDecryptRatchetClicks()
		{
			byte[] clicks = new byte[32];
			byte[] raw = secret.Output;
			int start = isInitiator ? 32 : 0;
			Array.Copy(raw, start, clicks, 0, 32);
			decryptRatchet.Clicks = clicks;
			//if (isInitiator) Console.WriteLine("Alice L CLICKS SET TO " + Transcoder.HexFromBytes(clicks));
			//else Console.WriteLine("Bob R CLICKS SET TO " + Transcoder.HexFromBytes(clicks));
		}

		/// <summary>
		/// Packages IV, ciphertext, and HMAC together, then
		/// turns the encryption ratchet.
		/// </summary>
		/// <param name="plainblob">The data to secure.</param>
		/// <returns>The secure message package.</returns>
		public byte[] Package(byte[] plainblob)
		{
			ThrowIfDisposed();
			try
			{
				// If secret is behind or equal
				if (state == State.Behind || state == State.Equal)
				{
					// Generate a new key
					secret.New();
				}

				byte[] localToken = LocalToken;

				// Packages the data
				byte[] box = new byte[4 + localToken.Length + plainblob.Length];
				Array.Copy(BitConverter.GetBytes(localToken.Length), 0, box, 0, 4);
				Array.Copy(localToken, 0, box, 4, localToken.Length);
				Array.Copy(plainblob, 0, box, 4 + localToken.Length, plainblob.Length);

				// Encrypts the data
				encryptRatchet.Turn();
				using var crypter = new RatchetCrypter(encryptRatchet.Output);
				var package = crypter.Package(box);

				if (state == State.Behind)  // If returning to equal state
				{
					secret.Turn(foreignToken);      // Both foreign and local have changed, so turn ratchet.
					UpdateEncryptRatchetClicks();   // Indicator has been sent, begin new encryption key next message
					state = State.Equal;
				}
				else if (state == State.Equal) // If moving ahead
				{
					UpdateEncryptRatchetClicks();   // New encryption can continue next message
					state = State.Ahead;
				}

				return package;
			}
			catch (Exception e)
			{
				Dispose();
				throw e;
			}
		}

		/// <summary>
		/// Verifies HMAC and decrypts message, then turns the decryption
		/// ratchet.
		/// </summary>
		/// <param name="package">The secure message package.</param>
		/// <exception cref="InvalidPackageException"/>
		/// <returns>The package contents.</returns>
		public byte[] Unpackage(byte[] package)
		{
			ThrowIfDisposed();
			try
			{
				decryptRatchet.Turn();

				using var crypter = new RatchetCrypter(decryptRatchet.Output);
				byte[] box = crypter.Unpackage(package);

				byte[] localTokenLength = new byte[4];
				Array.Copy(box, 0, localTokenLength, 0, localTokenLength.Length);

				byte[] tokenBytes = new byte[BitConverter.ToInt32(localTokenLength, 0)];
				Array.Copy(box, 4, tokenBytes, 0, tokenBytes.Length);

				byte[] plainblob = new byte[box.Length - 4 - tokenBytes.Length];
				Array.Copy(box, 4 + tokenBytes.Length, plainblob, 0, plainblob.Length);

				ECDHRatchet.Token token = new ECDHRatchet.Token(tokenBytes);

				if (token.IsDifferentTo(foreignToken))
				{
					foreignToken = token;
					if (state == State.Ahead)   // If returning to equal
					{
						secret.Turn(foreignToken);  // Both local and foreign have been changed, turn ratchet.
						UpdateDecryptRatchetClicks();   // Begin using
						state = State.Equal;
					}
					else if (state == State.Equal) // If falling behind
					{
						UpdateDecryptRatchetClicks(); // decrypt ratchet can update now
						state = State.Behind;
					}
				}
				return plainblob;
			}
			catch (Exception e)
			{
				Dispose();
				throw e;
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("DualRatchetCrypter has been disposed!");
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
				if (encryptRatchet != null) encryptRatchet.Dispose();
				if (decryptRatchet != null) decryptRatchet.Dispose();
			}

			disposed = true;
		}
	}
	*/
}