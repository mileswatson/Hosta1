﻿using Hosta.Crypto;
using Hosta.Exceptions;
using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to establish and send messages over an encrypted session.
	/// Does not guarantee message order unless commands are awaited.
	/// </summary>
	public class SecureConversation : IConversable
	{
		private static byte[] right = SecureRandomGenerator.GetBytes(32);
		private static byte[] left = SecureRandomGenerator.GetBytes(32);

		/// <summary>
		/// The insecure conversation to talk over.
		/// </summary>
		private readonly IConversable insecureConversation;

		/// <summary>
		/// The shared AES and HMAC key.
		/// </summary>
		private RatchetCrypter crypter;

		/// <summary>
		/// Ensures that only one task can send or receive at a time.
		/// </summary>
		private readonly AccessQueue accessQueue = new AccessQueue(1);

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
		/// <param name="initiated">
		/// Whether the underlying stream was initiated
		/// (rather than accepted).
		/// </param>
		/// <returns>An awaitable task.</returns>
		public Task Establish(bool initiated)
		{
			try
			{
				crypter = new RatchetCrypter(initiated ? right : left, initiated ? left : right);
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
		public async Task Send(byte[] data)
		{
			ThrowIfDisposed();
			await accessQueue.GetPass();
			try
			{
				byte[] secureMessage = crypter.Package(data);
				await insecureConversation.Send(secureMessage);
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
				return crypter.Unpackage(package);
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
				if (crypter != null) crypter.Dispose();
			}

			disposed = true;
		}
	}
}