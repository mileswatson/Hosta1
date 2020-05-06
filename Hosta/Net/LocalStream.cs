using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Net
{

	public class LocalStream : IStreamable
	{
		LocalStream contact;
		readonly Queue<byte> pendingBytes;
		readonly Queue<Tuple<int, TaskCompletionSource<byte[]>>> pendingReaders;
		

		public LocalStream()
		{
			pendingBytes = new Queue<byte>();
			pendingReaders = new Queue<Tuple<int, TaskCompletionSource<byte[]>>>();
		}

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
