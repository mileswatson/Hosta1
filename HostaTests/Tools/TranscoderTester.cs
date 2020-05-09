using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Hosta.Tools;
using Hosta.Crypto;

namespace HostaTests.Tools
{
	[TestClass]
	public class TranscoderTester
	{
		[DataTestMethod]
		[DataRow("")]
		[DataRow("plaintext")]
		[DataRow("1231232736458265919")]
		[DataRow(":'{}()(*&*%^%$#$#@!=_+")]
		public void BytesText_RoundTrip(string original)
		{
			byte[] encoded = Transcoder.BytesFromText(original);
			string novel = Transcoder.TextFromBytes(encoded);
			Assert.AreEqual(original, novel);
		}

		[TestMethod]
		public void BytesHex_RoundTrip()
		{
			for (int i = 0; i < 20; i++)
			{
				byte[] original = SecureRandomGenerator.GetBytes(i);
				string encoded = Transcoder.HexFromBytes(original);
				byte[] novel = Transcoder.BytesFromHex(encoded);
				CollectionAssert.AreEqual(original, novel);
			}
		}

		[DataTestMethod]
		[DataRow("1")]
		[DataRow("A2F")]
		public void HexToBytes_Invalid(string test)
		{
			Assert.ThrowsException<FormatException>(() =>
				Transcoder.BytesFromHex(test)
			);
		}
	}
}