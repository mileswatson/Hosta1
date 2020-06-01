using System;

namespace Hosta.Exceptions
{
	/// <summary>
	/// Indicates a secure message has been tampered with, could possibly indicate DoS.
	/// </summary>
	internal class InvalidPackageException : Exception
	{
		/// <summary>
		/// Constructs a new InvalidPackageException.
		/// </summary>
		/// <param name="message">Exception details.</param>
		public InvalidPackageException(string message) : base(message)
		{
		}
	}
}