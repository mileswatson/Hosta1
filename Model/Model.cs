using System.Diagnostics;

namespace Model
{
	internal class Model
	{
		private Database db;

		public Model()
		{
			db = new Database("database.sqlite");
		}

		public override string ToString()
		{
			return db.ToString();
		}
	}
}