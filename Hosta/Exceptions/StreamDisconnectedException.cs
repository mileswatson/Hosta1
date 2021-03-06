﻿using System;

namespace Hosta.Exceptions
{
	/// <summary>
	/// Indicates that a message was too large to send or receive, could indicate DoS.
	/// </summary>
	internal class StreamDisconnectedException : Exception
	{
		/// <summary>
		/// Constructs a new MessageTooLargeException.
		/// </summary>
		/// <param name="message">Exception details.</param>
		public StreamDisconnectedException(string message) : base(message)
		{
		}
	}
}