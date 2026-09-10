using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using Teleprompter.Data.Context;
using Teleprompter.Services.Packages;
using Teleprompter.Services.Parsing;
using Teleprompter.Services.Scraping;
using Teleprompter.Services.Settings;
using Teleprompter.Services.Transposition;
using Teleprompter.Wpf.ViewModels;

namespace Teleprompter.Wpf;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Data
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=teleprompter.db"));

        // Services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IJsonSongPackageService, JsonSongPackageService>();
        services.AddSingleton<IUniversalChordParserService, UniversalChordParserService>();
        services.AddSingleton<IChromaticTranspositionService, ChromaticTranspositionService>();
        services.AddTransient<ISongScraper, CifraClubScraper>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // Seed inicial se o banco estiver vazio
            if (!db.Songs.Any())
            {
                var scraper = scope.ServiceProvider.GetRequiredService<ISongScraper>();
                try
                {
                    string rawEvidencias = await scraper.ScrapeAsync("https://www.cifraclub.com.br/chitaozinho-e-xororo/evidencias/");
                    db.Songs.Add(new Teleprompter.Core.Models.Song 
                    { 
                        Title = "Evidências", 
                        Artist = "Chitãozinho & Xororó", 
                        Album = "Cowboy do Asfalto",
                        RawContent = rawEvidencias 
                    });
                    await db.SaveChangesAsync();
                }
                catch { /* fallback silencioso caso falhe a internet */ }
            }
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
