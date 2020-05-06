using Hosta.Exceptions;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hosta.Net
{

	/// <summary>
	/// Handles the sending and receiving of messages on stream.
	/// </summary>
	public class StreamMessenger : IConversable
	{
		readonly IStreamable stream;
		const int MaxBuffer = 1000; // max message size

		/// <summary>
		/// Creates MessageStreamer to wrap an underlying IStreamable stream.
		/// </summary>
		/// <param name="stream"></param>
		public StreamMessenger(IStreamable stream)
		{
			this.stream = stream;
		}


		/// <summary>
		/// No need to establish.
		/// </summary>
		/// <returns>
		/// A completed task.
		/// </returns>
		public Task Establish()
		{
			return Task.CompletedTask;
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
			if (length > MaxBuffer)
			{
				throw new MessageTooLargeException("A message was too large to be received!");
			}
			return await stream.Read(length);
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
			if (data.Length > MaxBuffer)
			{
				throw new MessageTooLargeException("A message was too large to be sent!");
			}
			await stream.Write(BitConverter.GetBytes(data.Length));
			await stream.Write(data);
		}
		
	}
}
