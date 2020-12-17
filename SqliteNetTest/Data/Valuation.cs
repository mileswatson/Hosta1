using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace SqliteNetTest.Data
{
	[Table("valuation")]
	public class Valuation
	{
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }

		[Indexed]
		[Column("stock_id")]
		public int StockId { get; set; }

		[Column("time")]
		public DateTime Time { get; set; }

		[Column("price")]
		public decimal Price { get; set; }
	}
}