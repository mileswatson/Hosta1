using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Static class for generating hashes and HMACs.
	/// </summary>
	public static class Hasher
	{
		/// <summary>
		/// Size of the HASH/HMAC in bytes.
		/// </summary>
		public const int OUTPUT_SIZE = 32;

		/// <summary>
		/// Hashes data.
		/// </summary>
		/// <param name="data">Data to hash.</param>
		/// <returns>The hash of the data.</returns>
		public static byte[] Hash(byte[] data)
		{
			using var hasher = SHA256.Create();
			return hasher.ComputeHash(data);
		}

		/// <summary>
		/// Calculates an HMAC.
		/// </summary>
		/// <param name="data">The data to calculate HMAC from.</param>
		/// <param name="key">The key to calculate HMAC with.</param>
		/// <returns>The HMAC of the data and key.</returns>
		public static byte[] HMAC(byte[] data, byte[] key)
		{
			using var hmac = new HMACSHA256(key);
			return hmac.ComputeHash(data);
		}
	}
}