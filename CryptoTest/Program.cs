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
			using var sc1 = new SecureConversation(cs1);
			using var sc2 = new SecureConversation(cs2);
			ls1.Connect(ls2);
			ls2.Connect(ls1);

			var a = sc1.Establish(false);
			var b = sc2.Establish(true);

			Task.WaitAll(a, b);

			a = sc2.Send(Encode("hello"));
			b = sc2.Send(Encode("hello1"));
			var c = sc2.Send(Encode("hello2"));
			var d = sc2.Send(Encode("hello3"));
			var e = sc1.Send(Encode("hello4"));
			var f = sc2.Send(Encode("hello5"));

			Console.WriteLine(Decode(sc1.Receive().Result));
			Console.WriteLine(Decode(sc1.Receive().Result));
			Console.WriteLine(Decode(sc1.Receive().Result));
			Console.WriteLine(Decode(sc1.Receive().Result));
			Console.WriteLine(Decode(sc2.Receive().Result));
			Console.WriteLine(Decode(sc1.Receive().Result));
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