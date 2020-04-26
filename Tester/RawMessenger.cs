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
		const int MaxBuffer = 8388608;

		public RawMessenger(Socket connectedSocket)
		{
			socket = connectedSocket;
		}

		public async Task<byte[]> ReceiveMessage()
		{
			int length = BitConverter.ToInt32(await ReadStream(8), 0);
			return await ReadStream(length);
		}

		public async Task SendMessage(byte[] data)
		{
			await WriteStream(BitConverter.GetBytes(data.Length));
			await WriteStream(data);
		}


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
