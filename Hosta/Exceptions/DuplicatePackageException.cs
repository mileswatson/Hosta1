using System;

namespace Hosta.Exceptions
{
	/// <summary>
	/// Indicates that a duplicate package has been received, could possible indicate DoS.
	/// </summary>
	internal class DuplicatePackageException : Exception
	{
		/// <summary>
		/// Constructs a new DuplicatePackageException.
		/// </summary>
		/// <param name="message">Exception details.</param>
		public DuplicatePackageException(string message) : base(message)
		{
		}
	}
}