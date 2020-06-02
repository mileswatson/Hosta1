using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Tools
{
	public class AccessQueue : IDisposable
	{
		/// <summary>
		/// Manages order of the queue.
		/// </summary>
		private readonly LinkedList<Tuple<Func<bool>, TaskCompletionSource<object>>> waitingTasks
			= new LinkedList<Tuple<Func<bool>, TaskCompletionSource<object>>>();

		/// <summary>
		/// Indicates whether to enforce order, or instead to let available tasks
		/// skip ahead of tasks with blocked conditions.
		/// </summary>
		private readonly bool enforceOrder;

		private int available = 0;
		private readonly int maximum = 0;

		/// <summary>
		/// Constructs a new AccessQueue.
		/// </summary>
		/// <param name="numConcurrentAccesses">The number of access cards.</param>
		/// <param name="enforceOrder">
		/// Whether to block the queue if the front condition fails.
		/// </param>
		public AccessQueue(int numConcurrentAccesses, bool enforceOrder = true)
		{
			this.enforceOrder = enforceOrder;
			available = numConcurrentAccesses;
			maximum = numConcurrentAccesses;
		}

		/// <summary>
		/// Queues for access to the resource.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		public Task GetPass()
		{
			ThrowIfDisposed();
			return GetPass(() => true);
		}

		/// <summary>
		/// Queues for access to the resource, will only go through
		/// when the condition returns true when a space is freed, or when
		/// CheckConditions() is manually called.
		/// </summary>
		/// <param name="condition">
		/// The condition that must return true for access to be granted.
		/// </param>
		/// <returns>An awaitable task.</returns>
		public Task GetPass(Func<bool> condition)
		{
			ThrowIfDisposed();
			var tcs = new TaskCompletionSource<object>();
			waitingTasks.AddLast(
				new Tuple<Func<bool>, TaskCompletionSource<object>>
					(condition, tcs));
			CheckForSpace();
			return tcs.Task;
		}

		/// <summary>
		/// Returns access to the resource.
		/// </summary>
		public void ReturnPass()
		{
			if (available == maximum)
			{
				throw new SemaphoreFullException("All passes have been given returned!");
			}
			available++;
			CheckForSpace();
		}

		/// <summary>
		/// Checks conditions, stops after first check if
		/// enforceOrder is true.
		/// </summary>
		/// <param name="t"></param>
		public void CheckForSpace()
		{
			Task.Run(() =>
			{
				lock (waitingTasks)
				{
					var currentNode = waitingTasks.First;
					while (currentNode != null && available > 0)
					{
						var tuple = currentNode.Value;
						if (tuple.Item1())
						{
							available--;
							waitingTasks.Remove(currentNode);
							tuple.Item2.SetResult(null);
						}
						else if (enforceOrder) return;
						currentNode = currentNode.Next;
					}
				}
			});
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("AccessQueue has been disposed!");
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
				CheckForSpace();
				if (waitingTasks != null)
				{
					lock (waitingTasks)
					{
						var currentNode = waitingTasks.First;
						while (currentNode != null)
						{
							var tuple = currentNode.Value;
							waitingTasks.Remove(currentNode);
							tuple.Item2.SetException(
								new ObjectDisposedException("AccessQueue has been disposed!"));
							currentNode = currentNode.Next;
						}
					}
				}
			}

			disposed = true;
		}
	}
}