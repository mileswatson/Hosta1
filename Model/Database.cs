using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace Model
{
	public class Database
	{
		
		public Database(string filename)
		{
			if (!File.Exists(filename))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
				File.Create(filename);
			}
			

			using (var conn = new SqliteConnection("Data Source=./Data/database.sqlite;Mode=ReadWriteCreate"))
			{
				conn.Open();
				SqliteCommand command = conn.CreateCommand();
				command.CommandText = @"CREATE TABLE IF NOT EXISTS users(
					id TEXT PRIMARY KEY,
					public_key TEXT NOT NULL UNIQUE,
					ip TEXT
				);";
				command.ExecuteNonQuery();
				Debug.WriteLine("Created.");
			}

		}

		

	}
}