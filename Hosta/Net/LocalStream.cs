using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Hosta.Exceptions;
using Hosta.Tools;

namespace Hosta.Net
{
	// Allows asynchronous local stream communications.
	public class LocalStream : IStreamable
	{
		/// <summary>
		/// The other LocalStream to write to and read from.
		/// </summary>
		private LocalStream contact;

		/// <summary>
		/// Allows for ordered, asynchronous sending of bytes.
		/// </summary>
		private readonly Queue<byte> pendingBytes = new Queue<byte>();

		/// <summary>
		/// Only allows one person to read at a time.
		/// </summary>
		private readonly AccessQueue readQueue = new AccessQueue(1);

		/// <summary>
		/// Only allows one person to write at a time.
		/// </summary>
		private readonly AccessQueue writeQueue = new AccessQueue(1);

		/// <summary>
		/// Constructs a new LocalStream.
		/// </summary>
		public LocalStream()
		{
		}

		/// <summary>
		/// Connects to another LocalStream.
		/// </summary>
		/// <param name="contact">The other LocalStream to connect to.</param>
		public void Connect(LocalStream contact)
		{
			ThrowIfDisposed();
			this.contact = contact;
		}

		/// <summary>
		/// Disconnects from the contact.
		/// </summary>
		public void Disconnect()
		{
			if (contact != null)
			{
				contact.contact = null;
				contact = null;
			}
		}

		/// <summary>
		/// Writes a byte[] to the output stream.
		/// </summary>
		/// <param name="blob">The byte[] to write.</param>
		/// <returns>An awaitable task.</returns>
		public async Task Write(byte[] blob)
		{
			ThrowIfDisposed();
			if (contact == null) throw new StreamDisconnectedException("The LocalStream has no valid contact!");

			await writeQueue.GetPass();
			try
			{
				foreach (byte b in blob) contact.pendingBytes.Enqueue(b);
			}
			catch (Exception e)
			{
				Dispose();
				throw e;
			}
			finally
			{
				writeQueue.ReturnPass();
				contact.readQueue.CheckForSpace();
			}
		}

		/// <summary>
		/// Reads a fixed number of bytes from the input stream.
		/// </summary>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>An awaitable task that resolves to the bytes that were read.</returns>
		public async Task<byte[]> Read(int size)
		{
			ThrowIfDisposed();
			await readQueue.GetPass(() => pendingBytes.Count >= size);
			try
			{
				byte[] blob = new byte[size];
				for (int i = 0; i < size; i++)
				{
					blob[i] = pendingBytes.Dequeue();
				}
				return blob;
			}
			catch (Exception e)
			{
				this.Dispose();
				throw e;
			}
			finally
			{
				readQueue.ReturnPass();
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("LocalStream has been disposed!");
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
				Disconnect();
				if (writeQueue != null) writeQueue.Dispose();
				if (readQueue != null) readQueue.Dispose();
				pendingBytes.Clear();
			}

			disposed = true;
		}
	}
}