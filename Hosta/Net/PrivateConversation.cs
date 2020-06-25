using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to establish and send messages over an encrypted session.
	/// Does not guarantee message order unless commands are awaited.
	/// </summary>
	public class PrivateConversation : IDisposable
	{
		private static readonly byte[] right = SecureRandomGenerator.GetBytes(32);
		private static readonly byte[] left = SecureRandomGenerator.GetBytes(32);

		/// <summary>
		/// The insecure conversation to talk over.
		/// </summary>
		private readonly ConversationStreamer insecureConversation;

		/// <summary>
		/// The shared AES and HMAC key.
		/// </summary>
		private RatchetCrypter crypter;

		/// <summary>
		/// Ensures that only one task can send or receive at a time.
		/// </summary>
		private readonly AccessQueue accessQueue = new AccessQueue(1);

		/// <summary>
		/// Marks whether the underlying stream was a requester or an accepter.
		/// </summary>
		public readonly bool isRequester;

		/// <summary>
		/// Constructs a new SecureConversation over an insecure ConversationStreamer.
		/// </summary>
		/// <param name="insecureConversation">
		/// The insecure conversation to talk over.
		/// </param>
		public PrivateConversation(IStreamable stream)
		{
			this.insecureConversation = new ConversationStreamer(stream);
			isRequester = stream.IsRequester;
		}

		/// <summary>
		/// Performs a key exchange across the insecure connection.
		/// </summary>
		/// <param name="initiated">
		/// Whether the underlying stream was initiated
		/// (rather than accepted).
		/// </param>
		/// <returns>An awaitable task.</returns>
		public Task Establish()
		{
			try
			{
				crypter = new RatchetCrypter(isRequester ? right : left, isRequester ? left : right);
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Dispose();
				throw e;
			}
		}

		/// <summary>
		/// Asynchronously sends an encrypted message. Does not guarantee
		/// order unless awaited.
		/// </summary>
		/// <param name="data">The message to encrypt and send.</param>
		/// <returns>An awaitable task.</returns>
		public Task Send(byte[] data)
		{
			ThrowIfDisposed();
			await accessQueue.GetPass();
			try
			{
				byte[] secureMessage = crypter.Encrypt(data);
				return insecureConversation.Send(secureMessage);
			}
			catch (Exception e)
			{
				this.Dispose();
				throw e;
			}
			finally
			{
				accessQueue.ReturnPass();
			}
		}

		/// <summary>
		/// Asynchronously receives an encrypted message. Does not guarantee
		/// order unless awaited.
		/// </summary>
		/// <returns>
		/// An awaitable task that resolves to the decrypted message.
		/// </returns>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();
			await accessQueue.GetPass();
			try
			{
				byte[] package = await insecureConversation.Receive();
				return crypter.Decrypt(package);
			}
			catch (Exception e)
			{
				this.Dispose();
				throw e;
			}
			finally
			{
				accessQueue.ReturnPass();
			}
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
				if (accessQueue != null) accessQueue.Dispose();
				if (insecureConversation != null) insecureConversation.Dispose();
			}

			disposed = true;
		}
	}
}
