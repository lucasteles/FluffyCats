using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        static readonly SemaphoreSlim semaphore = new SemaphoreSlim(5);

        public MainWindow() => InitializeComponent();
        async void Window_Loaded(object sender, RoutedEventArgs e) => await cuteLoop();
        async void AnimationCompleted(object sender, RoutedEventArgs e) => await cuteLoop();


        async Task cuteLoop()
        {
            await Task.WhenAny(
                    Enumerable.Range(0, 5 - cache.Count)
                    .Do(_ => semaphore.Wait())
                    .Select(async _ => cache.Push(await getFluffyCat()))
                    .Select(x => x.ContinueWith(_ => semaphore.Release()))
                    .Append(fillScreenWithLove())
                    );


        }

        async Task fillScreenWithLove()
        {
            string file;
            while (!cache.TryPop(out file))
                await Task.Delay(500);

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file);
            image.EndInit();

            SetAnimatedSource(imagemControle, image);
        }

        static async Task<string> getFluffyCat()
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
