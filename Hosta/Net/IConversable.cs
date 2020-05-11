using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// An interface that supports a simple asynchronous conversation using byte arrays.
	/// </summary>
	public interface IConversable : IDisposable
	{
		/// <summary>
		/// Asynchronously sends a byte[].
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <returns>An awaitable task.</returns>
		public Task Send(byte[] data);

		/// <summary>
		/// Asynchronously receives a byte[].
		/// </summary>
		/// <returns>An awaitable task that resolves to the received message.</returns>
		public Task<byte[]> Receive();
	}
}