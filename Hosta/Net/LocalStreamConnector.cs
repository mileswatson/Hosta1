using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/*
	public class LocalStreamConnector : IStreamConnector
	{
		private static Dictionary<string, LocalStreamConnector> connectors = new Dictionary<string, LocalStreamConnector>();

		private string address;
		private Queue<Tuple<IStreamable, TaskCompletionSource<object>>> pendingRequests;
		private Queue<TaskCompletionSource<IStreamable>> pendingAccepts;

		public void Bind(string address)
		{
			this.address = address;
			connectors.Add(this.address, this);
			pendingRequests = new Queue<Tuple<IStreamable, TaskCompletionSource<object>>>();
			pendingAccepts = new Queue<TaskCompletionSource<IStreamable>>();
		}

		public async Task<IStreamable> AcceptConnection(TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<IStreamable>();
			pendingAccepts.Enqueue(tcs);
			var finished = tcs.Task;
			var timedout = Task.Delay()
			return await tcs.Task;
		}

		public Task<IStreamable> RequestConnection(string address)
		{
			if (!connectors.ContainsKey(connectString))
			{
				throw new Exception("Address does not exist!");
			}
			LocalStream ls = new LocalStream();
			connectors[address].pendingRequests.Enqueue(new Tuple<>)
		}

		public void Dispose()
		{
		}
	}
	*/
}