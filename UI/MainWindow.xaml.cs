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

		bool created;

		public MainWindow()
		{

			//SQLiteConnection.CreateFile("hello.db)
			InitializeComponent();
		}

		private void ShowButton_Click(object sender, RoutedEventArgs e)
		{
			SqliteConnection connection = new SqliteConnection("Data Source=hello.db");
			if (!File.Exists("./hello.db")) File.Create("hello.db").Dispose();
			connection.Open();
			connection.Close();
			connection.Dispose();
		}
	}
}
