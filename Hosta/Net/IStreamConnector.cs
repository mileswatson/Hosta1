﻿using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	internal interface IStreamConnector : IDisposable
	{
		public void Bind(string bindingString);

		public Task<IStreamable> AcceptConnection();

		public Task<IStreamable> RequestConnection(string connectionString);
	}
}