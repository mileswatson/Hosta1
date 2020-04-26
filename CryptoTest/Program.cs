using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Program
{
	public static void Main()
	{
		TestECC();
		/*
		Console.WriteLine("start");
		Person alice = new Person();
		Person bob = new Person();

		alice.SendToken(bob);
		bob.SendToken(alice);

		Console.WriteLine(bob.Decrypt(alice.Encrypt("hello")));
		Console.WriteLine(alice.SendAuthentication(bob));
		Console.WriteLine(bob.SendAuthentication(alice));
		*/
	}

	public static void TestECC()
	{
		int numIterations = 10000;
		RandomNumberGenerator rng = RandomNumberGenerator.Create();
		byte[] hash = new byte[32];
		rng.GetBytes(hash);

		Stopwatch stopwatch = new Stopwatch();
		byte[] signature = new byte[0];
		ECDsaCng asymmetricKey = new ECDsaCng(521);
		signature = asymmetricKey.SignHash(hash);
		stopwatch.Start();
		for (int i = 0; i < numIterations; i++)
		{
			asymmetricKey.VerifyHash(hash, signature);
		}
		stopwatch.Stop();
		Console.WriteLine(((double)stopwatch.ElapsedMilliseconds) / ((double)numIterations));

		
		


	}

	public static void TestRSA()
	{
		int numIterations = 10000;
		RandomNumberGenerator rng = RandomNumberGenerator.Create();
		byte[] hash = new byte[32];
		rng.GetBytes(hash);

		Stopwatch stopwatch = new Stopwatch();
		byte[] signature = new byte[0];
		RSA asymmetricKey = RSA.Create(3072);
		signature = asymmetricKey.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		stopwatch.Start();
		for (int i = 0; i < numIterations; i++)
		{
			asymmetricKey.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		}
		stopwatch.Stop();
		Console.WriteLine(((double)stopwatch.ElapsedMilliseconds) / ((double)numIterations));





	}
}

public class Person
{

	private	ECDiffieHellmanCng localToken;
	private ECDsaCng identity;

	private byte[] sharedKey;
	

	public Person()
	{
		localToken = new ECDiffieHellmanCng(521);
		identity = new ECDsaCng(521);
		identity.HashAlgorithm = CngAlgorithm.Sha256;
	}

	public void SendToken(Person p)
	{
		p.ReceiveToken(localToken.PublicKey.ToByteArray());
	}

	public void ReceiveToken(byte[] b)
	{
		CngKey foreignToken = CngKey.Import(b, CngKeyBlobFormat.EccPublicBlob);
		sharedKey = localToken.DeriveKeyMaterial(foreignToken);
	}

	public bool SendAuthentication(Person p)
	{
		return p.ReceiveAuthentication(identity.Key.Export(CngKeyBlobFormat.EccPublicBlob), identity.SignData(sharedKey));
	}

	public bool ReceiveAuthentication(byte[] foreignPublicBlob, byte[] signature)
	{
		ECDsaCng foreignPublicKey = new ECDsaCng(CngKey.Import(foreignPublicBlob, CngKeyBlobFormat.EccPublicBlob));
		return foreignPublicKey.VerifyData(sharedKey, signature);
	}

	public byte[] Encrypt(string plaintext)
	{
		using (AesCng aes = new AesCng())
		{
			aes.Key = sharedKey;
			aes.IV = new byte[16];

			using (MemoryStream cipherstream = new MemoryStream())
			using (CryptoStream cryptostream = new CryptoStream(cipherstream, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
				byte[] plainblob = Encoding.UTF8.GetBytes(plaintext);
				cryptostream.Write(plainblob, 0, plainblob.Length);
				cryptostream.Close();
				return cipherstream.ToArray();
				
			}
		}
	}

	public string Decrypt(byte[] cipherblob)
	{
		using (AesCng aes = new AesCng())
		{
			aes.Key = sharedKey;
			aes.IV = new byte[16];
			using (MemoryStream plainstream = new MemoryStream())
			using (CryptoStream cryptostream = new CryptoStream(plainstream, aes.CreateDecryptor(), CryptoStreamMode.Write))
			{
				cryptostream.Write(cipherblob, 0, cipherblob.Length);
				cryptostream.Close();
				return Encoding.UTF8.GetString(plainstream.ToArray());
			}
		}
	}

}