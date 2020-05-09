using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hosta.Tools;

namespace Hosta.Net
{
	public class Program
	{
		public static void Main()
		{
			Console.WriteLine(Transcoder.TextFromBytes(new byte[] { 51, 0, 0, 32 }));
			LocalStream ls1 = new LocalStream();
			LocalStream ls2 = new LocalStream();

			ls1.Connect(ls2);
			ls2.Connect(ls1);

			ConversationStreamer ms1 = new ConversationStreamer(ls1);
			ConversationStreamer ms2 = new ConversationStreamer(ls2);

			SecureConversation sm1 = new SecureConversation(ms1);
			SecureConversation sm2 = new SecureConversation(ms2);

			Task.WaitAll(sm1.Establish(), sm2.Establish());

			sm1.Send(Encode("From sm1 - 1"));
			sm2.Send(Encode("From sm2 - 1"));

			Task<byte[]> received11 = sm1.Receive();
			Task<byte[]> received12 = sm1.Receive();

			sm1.Send(Encode("From sm1 - 2"));
			sm2.Send(Encode("From sm2 - 2"));

			Task<byte[]> received21 = sm2.Receive();
			Task<byte[]> received22 = sm2.Receive();

			Console.WriteLine(Decode(received11.Result));
			Console.WriteLine(Decode(received12.Result));
			Console.WriteLine(Decode(received21.Result));
			Console.WriteLine(Decode(received22.Result));
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