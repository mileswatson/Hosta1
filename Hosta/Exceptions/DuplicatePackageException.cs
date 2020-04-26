using System;
using System.Collections.Generic;
using System.Text;

namespace Hosta.Exceptions
{
	class DuplicatePackageException : Exception
	{
		public DuplicatePackageException(string message) : base(message)
		{ 
		}
	}
}
