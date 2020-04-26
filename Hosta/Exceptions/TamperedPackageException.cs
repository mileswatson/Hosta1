using System;
using System.Collections.Generic;
using System.Text;

namespace Hosta.Exceptions
{
	class TamperedPackageException : Exception
	{
		public TamperedPackageException(string message) : base(message)
		{
		}
	}
}
