using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// An APM to TAP wrapper for the default socket class.
	/// </summary>
	public class SocketStream : IStreamable
	{
		/// <summary>
		/// The system socket to communicate with.
		/// </summary>
		private readonly Socket socket;

		/// <summary>
		/// Constructs a new SocketStream from a connected socket.
		/// </summary>
		/// <param name="connectedSocket">
		/// The underlying socket to use.
		/// </param>
		public SocketStream(Socket connectedSocket)
		{
			socket = connectedSocket;
		}

		/// <summary>
		/// An APM to TAP wrapper for reading a fixed number of
		/// bytes from the TCP stream.
		/// </summary>
		/// <param name="size">
		///	The number of bytes to read from the TCP stream.
		/// </param>
		/// <returns>
		/// An awaitable task that resolves to the read blob.
		/// </returns>
		public Task<byte[]> Read(int size)
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
		/// bytes to the TCP stream.
		/// </summary>
		/// <param name="blob">The raw data to write to the TCP stream.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public Task Write(byte[] blob)
		{
			var tcs = new TaskCompletionSource<object>();
			socket.BeginSend(blob, 0, blob.Length, 0, ar =>
			{
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