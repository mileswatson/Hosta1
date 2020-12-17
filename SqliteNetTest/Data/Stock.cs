using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace SqliteNetTest.Data
{
	[Table("Stocks")]
	public class Stock
	{
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }

		[Column("symbol")]
		[Unique]
		public string Symbol { get; set; }

		[Column("creationTime")]
		public DateTime CreationTime { get; set; }
	}
}