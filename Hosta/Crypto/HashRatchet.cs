namespace Hosta.Crypto
{
	/// <summary>
	/// Offers key ratcheting functionality.
	/// </summary>
	public class HashRatchet
	{
		/// <summary>
		/// The hidden state of the ratchet.
		/// </summary>
		private byte[] secret;

		/// <summary>
		/// The value by which the ratchet should be turned.
		/// </summary>
		private byte[] clicks;

		/// <summary>
		/// Sets the value by which the ratchet should be turned.
		/// </summary>
		public byte[] Clicks {
			set {
				clicks = value;
			}
		}

		/// <summary>
		/// Gets the output of the ratchet.
		/// </summary>
		public byte[] Output {
			get {
				return Hasher.HMAC(new byte[32], secret);
			}
		}

		/// <summary>
		/// Constructs a new HashRatchet.
		/// </summary>
		/// <param name="clicks">The initial clicks to set.</param>
		public HashRatchet(byte[] clicks = null)
		{
			if (clicks == null) clicks = new byte[Hasher.OUTPUT_SIZE];
			this.secret = clicks;
		}

		/// <summary>
		/// Turns over the ratchet.
		/// </summary>
		public void Turn()
		{
			secret = Hasher.HMAC(secret, clicks);
		}
	}
}