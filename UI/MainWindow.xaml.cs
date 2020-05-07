using Model;
using System.Diagnostics;
using System.Windows;

namespace UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	public partial class MainWindow : Window
	{
		private Database db;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void ShowButton_Click(object sender, RoutedEventArgs e)
		{
			db = new Database("./Data/database.sqlite");
			Debug.WriteLine(db);
		}
	}
}