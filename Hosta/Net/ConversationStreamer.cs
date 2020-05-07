using Hosta.Exceptions;
using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Handles the sending and receiving of messages on stream.
	/// </summary>
	public class ConversationStreamer : IConversable
	{
		/// <summary>
		/// The underlying streams to read and write to.
		/// </summary>
		private readonly IStreamable stream;

		/// <summary>
		/// The maximum message size.
		/// </summary>
		private const int MaxSize = 1000;

		/// <summary>
		/// Creates MessageStreamer to wrap an underlying IStreamable stream.
		/// </summary>
		/// <param name="stream"></param>
		public ConversationStreamer(IStreamable stream)
		{
			this.stream = stream;
		}

		/// <summary>
		/// Asynchronous method that first sends the length
		/// of the message, then the message itself.
		/// </summary>
		/// <param name="data">The message to send.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public async Task Send(byte[] data)
		{
			if (data.Length > MaxSize)
			{
				throw new MessageTooLargeException("A message was too large to be sent!");
			}
			await stream.Write(BitConverter.GetBytes(data.Length));
			await stream.Write(data);
		}

		/// <summary>
		/// Asynchronous method that first reads the length,
		/// then the main message.
		/// </summary>
		/// <returns>
		/// An awaitable task that resolves to the message blob.
		/// </returns>
		public async Task<byte[]> Receive()
		{
			int length = BitConverter.ToInt32(await stream.Read(4), 0);
			if (length > MaxSize)
			{
				throw new MessageTooLargeException("A message was too large to be received!");
			}
			return await stream.Read(length);
		}
	}
}