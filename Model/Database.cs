using System;
using System.IO;

namespace Model
{
	public class Database
	{
		
		public Database()
		{
			if (!File.Exists("./Data/database.sqlite")) File.Create("./Data/database.sqlite");
		}

	}
}
