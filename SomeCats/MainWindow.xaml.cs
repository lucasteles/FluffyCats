using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static WpfAnimatedGif.ImageBehavior;

namespace SomeCats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly ConcurrentStack<string> cache = new ConcurrentStack<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.WhenAny(
                     Enumerable.Range(0, 3)
                     .Select(_ => LoadSomeCute())
                 );

            SetImage();
        }


        async Task LoadSomeCute()
        {
            var catFile = await GetFluffyCat();
            cache.Push(catFile);
        }

        void SetImage()
        {
            if (!cache.TryPop(out var file))
                return;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file);
            image.EndInit();

            SetAnimatedSource(imagemControle, image);
        }

        async void AnimationCompleted(object sender, RoutedEventArgs e)
        {
            SetImage();
            await LoadSomeCute();
        }

        static async Task<string> GetFluffyCat()
        {
            var file = Path.GetTempFileName();
            const string url = "https://cataas.com/cat/gif?filter=cute";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                var stream = await response.Content.ReadAsStreamAsync();
                using (var fs = File.Create(file))
                    stream.CopyTo(fs);

                return file;

            }
        }

    }
}
