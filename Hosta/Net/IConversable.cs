using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public interface IConversable
	{
		public Task Establish();
		public Task Send(byte[] data);
		public Task<byte[]> Receive();
	}
}
