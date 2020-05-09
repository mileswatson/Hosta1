using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using Hosta.Crypto;

namespace HostaTests.Crypto
{
	[TestClass]
	public class SecureRandomGeneratorTester
	{
		[TestMethod]
		public void DifferentEverytime()
		{
			List<byte[]> generated = new List<byte[]>();
			for (int i = 0; i < 100; i++)
			{
				generated.Add(SecureRandomGenerator.GetBytes(8));
				for (int j = 0; j < i; j++)
				{
					CollectionAssert.AreNotEqual(generated[i], generated[j]);
				}
			}
		}
	}
}