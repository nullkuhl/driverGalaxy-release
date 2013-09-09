using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Tuples;

namespace DriversGalaxy.Infrastructure
{
	public class NoiseGenerator : DependencyObject
	{
		static readonly DependencyPropertyKey noiseImageKey = DependencyProperty.RegisterReadOnly("NoiseImage", typeof(ImageSource), typeof(NoiseGenerator), new PropertyMetadata(null));
		//bool pendingChange = false;

		public NoiseGenerator()
		{
			this.Colors = new ObservableCollection<ColorFrequency>();
			this.Size = 100;
			GenerateNoiseBitmap();
		}

		void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			GenerateNoiseBitmap();
		}

		public ImageSource NoiseImage
		{
			get
			{
				return (ImageSource)GetValue(noiseImageKey.DependencyProperty);
			}
		}

		public int Size { get; set; }

		ObservableCollection<ColorFrequency> colorFrequency;
		public ObservableCollection<ColorFrequency> Colors
		{
			get { return colorFrequency; }
			set
			{
				if (colorFrequency == value)
				{
					return;
				}
				if (colorFrequency != null)
				{
					Colors.CollectionChanged -= Colors_CollectionChanged;
				}
				colorFrequency = value;
				if (colorFrequency != null)
				{
					Colors.CollectionChanged += Colors_CollectionChanged;
				}
			}
		}


		// BitmapSource generation code coutesy of http://social.msdn.microsoft.com/forums/en-US/wpf/thread/56364b28-1277-4be8-8d45-78788cc4f2d7/
		void GenerateNoiseBitmap()
		{
			if (Colors == null || Colors.Count == 0)
			{
				SetValue(noiseImageKey, null);
				return;
			}

			try
			{
				var rnd = new Random();
				var colors = Colors.Select(value => value.Color).ToList();
				var totalFrequency = Colors.Sum(a => a.Frequency);
				var frequencyMap = GetFrequencyMap();


				BitmapPalette palette = new BitmapPalette(colors);

				PixelFormat pf = PixelFormats.Bgra32;
				int width = Size;
				int height = width;

				int stride = (width * pf.BitsPerPixel) / 8;

				byte[] pixels = new byte[height * stride];


				for (int i = 0; i < height * stride; i += (pf.BitsPerPixel / 8))
				{
					var color = GetWeightedRandomColor(totalFrequency, frequencyMap, rnd);

					pixels[i] = color.B;
					pixels[i + 1] = color.G;
					pixels[i + 2] = color.R;
					pixels[i + 3] = color.A;
				}


				var image = BitmapSource.Create(width, height, 96, 96, pf, palette, pixels, stride);
				SetValue(noiseImageKey, image);
			}
			catch (ArgumentException)
			{
			}
		}

		Color GetWeightedRandomColor(int totalFrequency, List<Tuple<int, Color>> frequencyMap, Random rnd)
		{
			int value = rnd.Next(0, totalFrequency);
			for (int i = 0; i < frequencyMap.Count - 2; i++)
			{
				if (frequencyMap[i].Element1 < value && frequencyMap[i + 1].Element1 >= value)
				{
					return frequencyMap[i].Element2;
				}
			}
			return frequencyMap.Last().Element2;
		}

		List<Tuple<int, Color>> GetFrequencyMap()
		{
			var frequencyMap = new List<Tuple<int, Color>>();
			int counter = 0;
			foreach (var item in Colors)
			{
				frequencyMap.Add(new Tuple<int, Color>(counter, item.Color));
				counter += item.Frequency;
			}
			return frequencyMap;
		}
	}

	public class ColorFrequency
	{
		public Color Color { get; set; }
		public int Frequency { get; set; }
	}
}
