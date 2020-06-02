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
			using var lsc1 = new LocalStreamConnector();
			using var lsc2 = new LocalStreamConnector();

			lsc2.Bind("address1234");
			var a = lsc1.RequestConnection("address1234");
			var b = lsc2.AcceptConnection();

			using var sc1 = new PrivateConversation(a.Result);
			using var sc2 = new PrivateConversation(b.Result);

			var c = sc1.Establish();
			var d = sc2.Establish();

			Task.WaitAll(c, d);

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