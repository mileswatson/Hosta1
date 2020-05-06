using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class LocalMessenger : IConversable
	{
		LocalMessenger contact;
		readonly Queue<byte[]> pendingMessages;
		readonly Queue<TaskCompletionSource<byte[]>> pendingReaders;

		public LocalMessenger()
		{
			pendingMessages = new Queue<byte[]>();
			pendingReaders = new Queue<TaskCompletionSource<byte[]>>();
		}

		public void Connect(LocalMessenger contact)
		{
			this.contact = contact;
		}

		public Task Establish()
		{
			return Task.CompletedTask;
		}

		public Task Send(byte[] data)
		{
			contact.pendingMessages.Enqueue(data);
			contact.HandleInputOperations();
			return Task.CompletedTask;
		}

		public Task<byte[]> Receive()
		{
			var tcs = new TaskCompletionSource<byte[]>();
			pendingReaders.Enqueue(tcs);
			HandleInputOperations();
			return tcs.Task;
		}

		public void HandleInputOperations()
		{
			lock (pendingReaders)
			{
				while (pendingMessages.Count > 0 && pendingReaders.Count > 0)
				{
					pendingReaders.Dequeue().SetResult(pendingMessages.Dequeue());
				}
			}
		}
	}
}
