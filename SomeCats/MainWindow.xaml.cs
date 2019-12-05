using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static WpfAnimatedGif.ImageBehavior;
using CatFileName = System.String;


namespace SomeCats
{
    public partial class MainWindow : Window
    {
        const int MAX_GIFS = 5;

        static ChannelReader<CatFileName> catReader;
        static HttpClient httpClient = new HttpClient();

        public MainWindow() => InitializeComponent();

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var channel = Channel.CreateBounded<string>(MAX_GIFS);
            catReader = channel.Reader;

            _ = Task.Run(() => cuteLoop(channel.Writer));
            await fillScreenWithLove();
        }

        async void AnimationCompleted(object sender, RoutedEventArgs e) => await fillScreenWithLove();

        async static Task cuteLoop(ChannelWriter<string> catWriter)
        {
            async Task WriteCat()
            {
                while (true) await catWriter.WriteAsync(await getFluffyCat());
            };

            await Task.WhenAll(
                    Enumerable
                        .Range(0, MAX_GIFS)
                        .Select(_ => WriteCat()));

        }

        async Task fillScreenWithLove()
        {
            var file = await catReader.ReadAsync();

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file);
            image.EndInit();

            SetAnimatedSource(imagemControle, image);
        }

        static async Task<CatFileName> getFluffyCat()
        {
            var file = Path.GetTempFileName();
            const string url = "https://cataas.com/cat/gif?filter=cute";

            var response = await httpClient.GetAsync(url);
            Debug.WriteLine("Mais um tanananan");

            var stream = await response.Content.ReadAsStreamAsync();
            using (var fs = File.Create(file))
                stream.CopyTo(fs);

            return file;
        }

    }
}
