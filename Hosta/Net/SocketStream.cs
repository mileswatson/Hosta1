using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Hosta.Exceptions;
using Hosta.Tools;

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

		private readonly AccessQueue readQueue = new AccessQueue(1);

		private readonly AccessQueue writeQueue = new AccessQueue(1);

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
		public async Task<byte[]> Read(int size)
		{
			ThrowIfDisposed();
			await readQueue.GetPass();
			try
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
					finally
					{
						readQueue.ReturnPass();
					}
				}, null);
				return await tcs.Task;
			}
			catch (Exception e)
			{
				Dispose();
				throw e;
			}
			finally
			{
				readQueue.ReturnPass();
			}
		}

		/// <summary>
		/// An APM to TAP wrapper for writing
		/// bytes to the TCP stream.
		/// </summary>
		/// <param name="blob">The raw data to write to the TCP stream.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public async Task Write(byte[] blob)
		{
			ThrowIfDisposed();
			await writeQueue.GetPass();
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
				finally
				{
					writeQueue.ReturnPass();
				}
			}, null);
			await tcs.Task;
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("SocketStream has been disposed!");
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
				if (readQueue != null) readQueue.Dispose();
				if (writeQueue != null) writeQueue.Dispose();
				socket.Close();
			}

			disposed = true;
		}
	}
}