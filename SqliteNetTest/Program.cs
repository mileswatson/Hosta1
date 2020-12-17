using System;
using SqliteNetTest.Data;

namespace SqliteNetTest
{
	public class Program
	{
		private static void Main(string[] args)
		{
			DatabaseHandler db = new DatabaseHandler(
				@"C:\Users\Miles\Documents\Documents\Programming\NEA\Hosta1\SqliteNetTest\.persistence\market.db");
			db.AddStock("MSFT");
			db.AddStock("APPL");
			db.AddStock("DLBY");
			db.AddStock("DOWJ");
			db.AddStock("EXON");
			foreach (var stock in db.GetStocks())
			{
				Console.WriteLine(stock.Symbol);
			}
		}
	}
}