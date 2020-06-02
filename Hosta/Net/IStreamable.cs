using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// An interface that supports reading an writing fixed sized items to a stream.
	/// </summary>
	public interface IStreamable : IDisposable
	{
		public bool IsRequester {
			get;
		}

		/// <summary>
		/// Writes a byte[] to the output stream.
		/// </summary>
		/// <param name="data"> The data to write.</param>
		/// <returns>An awaitable task.</returns>
		public Task Write(byte[] data);

		/// <summary>
		/// Reads a set number of bytes from the input stream.
		/// </summary>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>An awaitable task that resolves to the bytes that were read.</returns>
		public Task<byte[]> Read(int size);
	}
}