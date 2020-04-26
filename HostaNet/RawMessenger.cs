using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostaNet
{
	/// <summary>
	/// Handles sending and receiving of bytes.
	/// </summary>
	internal class RawMessenger
	{

		Socket socket;
		const int MaxBuffer = 8388608; // max message size

		public RawMessenger(Socket connectedSocket)
		{
			socket = connectedSocket;
		}

		/// <summary>
		/// Asynchronous method that first reads the length 
		/// into a 4 byte buffer, then reads the main message.
		/// </summary>
		/// <returns>
		/// An awaitable task that resolves to the message blob.
		/// </returns>
		public async Task<byte[]> Receive()
		{
			int length = BitConverter.ToInt32(await ReadStream(4), 0);
			return await ReadStream(length);
		}

		/// <summary>
		/// Asynchronous method that first send the length 
		/// of the message, then the message itself.
		/// </summary>
		/// <param name="data">The message to send.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public async Task Send(byte[] data)
		{
			await WriteStream(BitConverter.GetBytes(data.Length));
			await WriteStream(data);
		}

		/// <summary>
		/// An APM to TAP wrapper for reading a fixed number of
		/// bytes from the tcp stream.
		/// </summary>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>
		/// An awaitable task that resolves to Int32.
		/// </returns>
		public Task<byte[]> ReadStream(int size)
		{
			byte[] buffer = new byte[size];
			var tcs = new TaskCompletionSource<byte[]>();
			socket.BeginReceive(buffer, 0, size, 0, ar =>
			{
				try
				{
					socket.EndReceive(ar);
					tcs.SetResult(buffer);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);
			return tcs.Task;
		}

		/// <summary>
		/// An APM to TAP wrapper for writing
		/// bytes to the tcp stream.
		/// </summary>
		/// <param name="data">The raw data to write to the tcp stream.</param>
		/// <returns></returns>
		public Task WriteStream(byte[] data)
		{
			var tcs = new TaskCompletionSource<object>();
			socket.BeginSend(data, 0, data.Length, 0, ar => { 
				try
				{
					socket.EndSend(ar);
					tcs.SetResult(null);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);
			return tcs.Task;
		}

	}
}
