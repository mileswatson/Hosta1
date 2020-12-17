using Model;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Windows.Media.Imaging;

namespace UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Thread.Sleep(2000);
			var rect = new Rectangle
			{
				Height = 1,
				Width = double.NaN,
				Stroke = Brushes.Black
			};
			posts.Children.Add(rect);

			Stream imageStreamSource = new FileStream(@"C:\Users\Miles\Documents\Documents\Programming\NEA\Hosta1\UI\resources\hosta_logo.png", FileMode.Open, FileAccess.Read, FileShare.Read);
			PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
			BitmapSource bitmapSource = decoder.Frames[0];

			for (int i = 0; i < 1000; i++)
			{
				var dp = new DockPanel();

				// Draw the Image
				var profile = new Image()
				{
					Width = 40,
					Height = 40,
					Source = bitmapSource,
					Stretch = Stretch.Uniform,
					Margin = new Thickness(20)
				};
				DockPanel.SetDock(profile, Dock.Left);
				dp.Children.Add(profile);

				var name = new Label
				{
					Content = "Miles Watson" + i.ToString(),
					FontWeight = FontWeights.Bold
				};
				DockPanel.SetDock(name, Dock.Top);
				dp.Children.Add(name);

				var tb = new TextBlock
				{
					TextWrapping = TextWrapping.WrapWithOverflow,
					Padding = new Thickness(0, 0, 0, 20),
					Text = "This is a cool platform!"
				};
				DockPanel.SetDock(tb, Dock.Top);
				dp.Children.Add(tb);

				posts.Children.Add(dp);

				rect = new Rectangle
				{
					Height = 1,
					Width = double.NaN,
					Stroke = Brushes.Black
				};
				posts.Children.Add(rect);
			}
		}
	}
}