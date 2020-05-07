using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Allows asynchronous local conversations.
	/// </summary>
	public class LocalConversation : IConversable
	{
		/// <summary>
		/// The other LocalConversation to send/receive messages to/from.
		/// </summary>
		private LocalConversation contact;

		/// <summary>
		/// A queue of messages to be read.
		/// </summary>
		private readonly Queue<byte[]> pendingMessages;

		/// <summary>
		/// A queue of waiting readers.
		/// </summary>
		private readonly Queue<TaskCompletionSource<byte[]>> pendingReaders;

		/// <summary>
		/// Initialises message and reader queues.
		/// </summary>
		public LocalConversation()
		{
			pendingMessages = new Queue<byte[]>();
			pendingReaders = new Queue<TaskCompletionSource<byte[]>>();
		}

		/// <summary>
		/// Connects to another LocalConversation.
		/// </summary>
		/// <param name="contact">The other LocalConversation to connect to.</param>
		public void Connect(LocalConversation contact)
		{
			this.contact = contact;
		}

		public Task Send(byte[] data)
		{
			contact.pendingMessages.Enqueue(data);
			contact.HandlePendingReaders();
			return Task.CompletedTask;
		}

		public Task<byte[]> Receive()
		{
			var tcs = new TaskCompletionSource<byte[]>();
			pendingReaders.Enqueue(tcs);
			HandlePendingReaders();
			return tcs.Task;
		}

		/// <summary>
		/// Used to ensure that message order is maintained.
		/// </summary>
		public void HandlePendingReaders()
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