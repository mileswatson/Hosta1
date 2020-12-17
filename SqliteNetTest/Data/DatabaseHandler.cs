using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Linq;

namespace SqliteNetTest.Data
{
	public class DatabaseHandler
	{
		private SQLiteConnection _db;

		public DatabaseHandler(string path)
		{
			_db = new SQLiteConnection(path);
			_db.DropTable<Stock>();
			_db.DropTable<Valuation>();
			_db.CreateTable<Stock>();
			_db.CreateTable<Valuation>();
		}

		public void AddStock(string id)
		{
			_db.Insert(
				new Stock
				{
					Symbol = id,
					CreationTime = DateTime.Now
				}
			);
		}

		public List<Stock> GetStocks()
		{
			var stocks = _db.Table<Stock>()
				.OrderByDescending<DateTime>(s => s.CreationTime)
				.ToList();
			return stocks;
		}
	}
}