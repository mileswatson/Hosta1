using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Net
{
	internal interface IStreamConnector : IDisposable
	{
		public void Bind(string bindingString);

		public Task<IStreamable> AcceptConnection(Task cancelled);

		public Task<IStreamable> Connect(string connectionString);
	}
}