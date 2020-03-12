using System;
using System.Windows;

using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;

namespace UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>


	public partial class MainWindow : Window
	{

        SqliteConnection connection;
		bool created;

		public MainWindow()
		{

			//SQLiteConnection.CreateFile("hello.db)
			InitializeComponent();
		}

		private void showButton_Click(object sender, RoutedEventArgs e)
		{
			connection = new SqliteConnection("Data Source=hello.db");
			if (!File.Exists("./hello.db")) File.Create("hello.db");
			connection.Open();
			connection.Close();

		}
	}
}
