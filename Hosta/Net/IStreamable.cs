using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public interface IStreamable
	{
		public Task<byte[]> Read(int size);
		public Task Write(byte[] data);
	}
}
