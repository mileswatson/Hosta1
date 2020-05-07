using System;

namespace Hosta.Exceptions
{
	/// <summary>
	/// Indicates a secure message has been tampered with, could possibly indicate DoS.
	/// </summary>
	internal class TamperedPackageException : Exception
	{
		/// <summary>
		/// Constructs a new TamperedPackageException.
		/// </summary>
		/// <param name="message">Exception details.</param>
		public TamperedPackageException(string message) : base(message)
		{
		}
	}
}