using LearningWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LearningWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private PostsViewModel postsVM = new PostsViewModel();

		private ProfileViewModel profileVM = new ProfileViewModel();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Posts_Clicked(object sender, RoutedEventArgs e)
		{
			DataContext = postsVM;
		}

		private void Profile_Clicked(object sender, RoutedEventArgs e)
		{
			DataContext = profileVM;
		}
	}
}