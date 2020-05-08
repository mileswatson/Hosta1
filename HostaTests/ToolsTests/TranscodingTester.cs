using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Hosta.Tools;

namespace HostaTests.ToolsTests
{
	[TestClass]
	public class TranscodingTester
	{
		[DataTestMethod]
		[DataRow("")]
		[DataRow("plaintext")]
		[DataRow("1231232736458265919")]
		[DataRow(":'{}()(*&*%^%$#$#@!=_+")]
		public void Text_RoundTrip(string original)
		{
			byte[] encoded = Transcoding.FromText(original);
			string novel = Transcoding.GetText(encoded);
			Assert.AreEqual(original, novel);
		}

		[TestMethod]
		public void Hex_RoundTrip()
		{
			for (int i = 0; i < 20; i++)
			{
				byte[] original = Crypto.SecureRandomBytes(i);
				string encoded = Transcoding.GetHex(original);
				byte[] novel = Transcoding.FromHex(encoded);
				CollectionAssert.AreEqual(original, novel);
			}
		}
	}
}