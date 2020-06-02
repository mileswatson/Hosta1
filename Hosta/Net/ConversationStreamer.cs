using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Hosta.Exceptions;
using Hosta.Tools;

namespace Hosta.Net
{
	/// <summary>
	/// Handles the sending and receiving of messages on stream.
	/// Does not guarantee message order if commands are not awaited.
	/// </summary>
	public class ConversationStreamer : IDisposable
	{
		/// <summary>
		/// The underlying streams to read and write to.
		/// </summary>
		private readonly IStreamable stream;

		/// <summary>
		/// The maximum message size.
		/// </summary>
		private const int MaxSize = 1000;

		private readonly AccessQueue sendQueue = new AccessQueue(1);

		private readonly AccessQueue receiveQueue = new AccessQueue(1);

		/// <summary>
		/// Creates MessageStreamer to wrap an underlying IStreamable stream.
		/// </summary>
		/// <param name="stream">The underlying stream to use.</param>
		public ConversationStreamer(IStreamable stream)
		{
			this.stream = stream;
		}

		/// <summary>
		/// Method that first sends the length
		/// of the message, then the message itself.
		/// </summary>
		/// <param name="data">The message to send.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public async Task Send(byte[] data)
		{
			ThrowIfDisposed();
			await sendQueue.GetPass();
			try
			{
				if (data.Length > MaxSize)
				{
					throw new MessageTooLargeException("A message was too large to be sent!");
				}
				await Task.WhenAll(
					stream.Write(BitConverter.GetBytes(data.Length)),
					stream.Write(data)
				);
			}
			catch (Exception e)
			{
				this.Dispose();
				throw e;
			}
			finally
			{
				sendQueue.ReturnPass();
			}
		}

		/// <summary>
		/// Asynchronous method that reads first the length
		/// then the main message from a stream.
		/// </summary>
		/// <returns>
		/// An awaitable task that resolves to the message blob.
		/// </returns>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();
			await receiveQueue.GetPass();
			try
			{
				int length = BitConverter.ToInt32(await stream.Read(4), 0);
				if (length > MaxSize)
				{
					throw new MessageTooLargeException("A message was too large to be received!");
				}
				return await stream.Read(length);
			}
			catch (Exception e)
			{
				this.Dispose();
				throw e;
			}
			finally
			{
				receiveQueue.ReturnPass();
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("ConversationStreamer has been disposed!");
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
				if (stream != null) stream.Dispose();
				if (sendQueue != null) sendQueue.Dispose();
				if (receiveQueue != null) receiveQueue.Dispose();
			}

			disposed = true;
		}
	}
}