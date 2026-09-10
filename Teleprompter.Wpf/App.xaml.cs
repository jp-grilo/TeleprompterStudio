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

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Certificar que o banco está criado
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
