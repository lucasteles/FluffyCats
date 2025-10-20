using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SomeCats;

using static WpfAnimatedGif.ImageBehavior;
using CatFileName = string;

public partial class MainWindow
{
    const int MaxGifs = 5;
    const CatFileName Url = "https://cataas.com/cat/gif?position=center";
    static readonly HttpClient HttpClient = new();
    readonly Channel<CatFileName> channel;
    readonly CancellationTokenSource cts = new();
    Task? loopTask;

    public MainWindow()
    {
        InitializeComponent();
        channel = Channel.CreateBounded<CatFileName>(MaxGifs);
    }

    void Window_Loaded(object sender, RoutedEventArgs e)
    {
        loopTask = Task.Run(CuteLoop, cts.Token);
        NextGif();
    }

    void Window_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (cts.IsCancellationRequested) return;
        cts.Cancel();
        if (loopTask?.IsCompleted is false)
            loopTask?.GetAwaiter().GetResult();
    }

    void AnimationCompleted(object sender, RoutedEventArgs e) => NextGif();

    async Task CuteLoop() =>
        await Task.WhenAll(
            Enumerable
                .Range(0, MaxGifs)
                .Select(async _ =>
                {
                    while (!cts.IsCancellationRequested)
                        await channel.Writer.WriteAsync(await GetFluffyCat(), cts.Token);
                }));

    async void NextGif()
    {
        try
        {
            await FillScreenWithLove();
        }
        catch (Exception exception)
        {
            Console.Error.Write(exception);
        }
    }

    async Task FillScreenWithLove()
    {
        var file = await channel.Reader.ReadAsync();
        BitmapImage image = new();
        image.BeginInit();
        image.UriSource = new(file);
        image.EndInit();
        SetAnimatedSource(imagemControle, image);
    }

    static async Task<CatFileName> GetFluffyCat(CancellationToken ct = default)
    {
        var file = Path.GetTempFileName();
        var response = await HttpClient.GetAsync(Url, ct);
        Debug.WriteLine("Mais um tananam!");

        var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var fs = File.Create(file);
        await stream.CopyToAsync(fs, ct);
        return file;
    }
}