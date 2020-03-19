using System;
using System.Windows;

using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using Model;

namespace UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>


	public partial class MainWindow : Window
	{

		bool created;
		Database db;

		public MainWindow()
		{

			
			InitializeComponent();
		}

		private void ShowButton_Click(object sender, RoutedEventArgs e)
		{
			db = new Database("./Data/database.sqlite");
		}
	}
}
