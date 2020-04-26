using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace HostaNet
{
	class SecureMessenger
	{
		RawMessenger rawMessenger;
		byte[] key;

		public SecureMessenger(RawMessenger rawMessenger)
		{
			this.rawMessenger = rawMessenger;
		}

		public async Task Secure()
		{
			ECDiffieHellmanCng cng = n;
			rawMessenger.SendMessage()
		}
	}
}
