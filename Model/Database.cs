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
			
			SqliteConnection conn = new SqliteConnection("Data Source=./Data/database.sqlite;Mode=ReadWriteCreate");
			conn.Open();
			SqliteCommand command = conn.CreateCommand();
			command.CommandText = @"SELECT COUNT(name) FROM sqlite_master WHERE type = 'table' AND name = 'users';";
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				if (reader.GetInt32(0) == 0) {
					Debug.WriteLine("Not created");
				}
			}
			conn.Close();

		}

		

	}
}