using System;

namespace Hosta.Exceptions
{
	/// <summary>
	/// Thrown to indicate that a cryptographic parameter was
	/// not in the correct format.
	/// </summary>
	public class CryptoParameterException : Exception
	{
		/// <summary>
		/// Constructs a new DuplicatePackageException.
		/// </summary>
		/// <param name="message">Exception details.</param>
		public CryptoParameterException(string message) : base(message)
		{
		}
	}
}