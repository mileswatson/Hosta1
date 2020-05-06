using System;
using System.Collections.Generic;
using System.Text;

namespace Hosta.Exceptions
{
	class MessageTooLargeException : Exception
	{
		public MessageTooLargeException(string message) : base(message)
		{
		}
	}
}
