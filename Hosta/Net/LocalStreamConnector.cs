using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class LocalStreamConnector : IStreamConnector
	{
		private static readonly Dictionary<string, LocalStreamConnector> connectors = new Dictionary<string, LocalStreamConnector>();

		private string address;
		private Queue<TaskCompletionSource<IStreamable>> pendingRequests;
		private Queue<TaskCompletionSource<IStreamable>> pendingAccepts;

		public void Bind(string address)
		{
			ThrowIfDisposed();
			this.address = address;
			connectors.Add(this.address, this);
			pendingRequests = new Queue<TaskCompletionSource<IStreamable>>();
			pendingAccepts = new Queue<TaskCompletionSource<IStreamable>>();
		}

		public Task<IStreamable> AcceptConnection()
		{
			ThrowIfDisposed();
			if (pendingRequests.Count > 0)
			{
				var request = pendingRequests.Dequeue();
				LocalStream accepter = new LocalStream(false);
				LocalStream requester = new LocalStream(true);
				accepter.Connect(requester);
				requester.Connect(accepter);
				request.SetResult(requester);
				return Task<IStreamable>.FromResult(accepter as IStreamable);
			}
			var tcs = new TaskCompletionSource<IStreamable>();
			pendingAccepts.Enqueue(tcs);
			return tcs.Task;
		}

		public Task<IStreamable> RequestConnection(string address)
		{
			ThrowIfDisposed();
			if (!connectors.ContainsKey(address))
			{
				throw new Exception("Address has not been bound!");
			}
			var connector = connectors[address];
			if (connector.pendingAccepts.Count > 0)
			{
				var accept = connector.pendingAccepts.Dequeue();
				LocalStream accepter = new LocalStream(false);
				LocalStream requester = new LocalStream(true);
				accepter.Connect(requester);
				requester.Connect(accepter);
				accept.SetResult(accepter);
				return Task<IStreamable>.FromResult(requester as IStreamable);
			}
			var tcs = new TaskCompletionSource<IStreamable>();
			connector.pendingRequests.Enqueue(tcs);
			return tcs.Task;
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("LocalStreamConnector has been disposed!");
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
				if (pendingAccepts != null)
					while (pendingAccepts.Count > 0) pendingAccepts.Dequeue()
						.SetException(new ObjectDisposedException("LocalStreamConnector has been disposed!"));
				if (pendingRequests != null)
					while (pendingRequests.Count > 0) pendingRequests.Dequeue()
							.SetException(new ObjectDisposedException("LocalStreamConnector has been disposed!"));
			}

			disposed = true;
		}
	}
}