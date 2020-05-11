using Hosta.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		/// A queue of bytes to be read.
		/// </summary>
		private Queue<byte> pendingBytes;

		/// <summary>
		/// A queue of waiting readers.
		/// </summary>
		private Queue<Tuple<int, TaskCompletionSource<byte[]>>> pendingReaders;

		/// <summary>
		/// Constructs a new LocalStream.
		/// </summary>
		public LocalStream()
		{
			pendingBytes = new Queue<byte>();
			pendingReaders = new Queue<Tuple<int, TaskCompletionSource<byte[]>>>();
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
		public Task Write(byte[] blob)
		{
			ThrowIfDisposed();
			if (contact == null) throw new StreamDisconnectedException("The LocalStream has no valid contact!");

			return Task.Run(() =>
			{
				lock (pendingBytes)
				{
					foreach (byte b in blob)
					{
						contact.pendingBytes.Enqueue(b);
					}
				}
				contact.HandlePendingReaders();
			});
		}

		/// <summary>
		/// Reads a fixed number of bytes from the input stream.
		/// </summary>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>An awaitable task that resolves to the bytes that were read.</returns>
		public Task<byte[]> Read(int size)
		{
			ThrowIfDisposed();
			var tcs = new TaskCompletionSource<byte[]>();
			pendingReaders.Enqueue(new Tuple<int, TaskCompletionSource<byte[]>>(size, tcs));
			HandlePendingReaders();
			return tcs.Task;
		}

		/// <summary>
		/// Used to ensure that stream order is maintained.
		/// </summary>
		public void HandlePendingReaders()
		{
			lock (pendingReaders)
			{
				while (pendingReaders.Count > 0 && pendingBytes.Count >= pendingReaders.Peek().Item1)
				{
					var job = pendingReaders.Dequeue();
					int size = job.Item1;
					TaskCompletionSource<byte[]> tcs = job.Item2;
					byte[] output = new byte[size];
					for (int i = 0; i < size; i++)
					{
						output[i] = pendingBytes.Dequeue();
					}
					tcs.SetResult(output);
				}
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

				lock (pendingReaders)
				{
					foreach (var t in pendingReaders)
					{
						pendingReaders.Dequeue().Item2.TrySetException(
							new OperationCanceledException("LocalStream has been disposed of before message could be received."));
					}
					pendingReaders = null;
				}
			}

			disposed = true;
			Console.WriteLine("LocalStream disposed!");
		}
	}
}