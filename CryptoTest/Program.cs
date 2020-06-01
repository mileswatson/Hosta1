using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Hosta.Crypto;
using Hosta.Tools;

namespace Hosta.Net
{
	public class Program
	{
		public static void Main()
		{
			using var ls1 = new LocalStream();
			using var ls2 = new LocalStream();
			using var cs1 = new ConversationStreamer(ls1);
			using var cs2 = new ConversationStreamer(ls2);
			using var sc1 = new PrivateConversation(cs1);
			using var sc2 = new PrivateConversation(cs2);
			ls1.Connect(ls2);
			ls2.Connect(ls1);

			sc1.Establish(true);
			sc2.Establish(false);

			sc1.Send(Encode("r1")).Wait();
			sc1.Send(Encode("r2")).Wait();

			sc2.Send(Encode("l1")).Wait();
			Console.WriteLine(Decode(sc2.Receive().Result));
			sc2.Send(Encode("l2")).Wait();
			Console.WriteLine(Decode(sc2.Receive().Result));

			Console.WriteLine(Decode(sc1.Receive().Result));
			sc1.Send(Encode("r3")).Wait();
			Console.WriteLine(Decode(sc1.Receive().Result));
			sc1.Send(Encode("r4")).Wait();
			sc1.Send(Encode("r5")).Wait();

			Console.WriteLine(Decode(sc2.Receive().Result));
			Console.WriteLine(Decode(sc2.Receive().Result));
			Console.WriteLine(Decode(sc2.Receive().Result));
		}

		public static byte[] Encode(string data)
		{
			return Encoding.UTF8.GetBytes(data);
		}

		public static string Decode(byte[] plainbytes)
		{
			return Encoding.UTF8.GetString(plainbytes);
		}
	}
}