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
		private readonly Queue<byte> pendingBytes;

		/// <summary>
		/// A queue of waiting readers.
		/// </summary>
		private readonly Queue<Tuple<int, TaskCompletionSource<byte[]>>> pendingReaders;

		/// <summary>
		/// Initialises byte and reader queues.
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
			this.contact = contact;
		}

		public Task Write(byte[] blob)
		{
			return Task.Run(() =>
			{
				foreach (byte b in blob)
				{
					contact.pendingBytes.Enqueue(b);
				}
				contact.HandlePendingReaders();
			});
		}

		public Task<byte[]> Read(int size)
		{
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
	}
}