using System.Text.Json;

namespace Teleprompter.Services.Settings;

public class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public double DefaultScrollSpeed { get; set; } = 1.0;
    public string DefaultNotation { get; set; } = "Anglo";
    public string DatabasePath { get; set; } = "teleprompter.db";
}

public interface ISettingsService
{
    AppSettings Current { get; }
    Task LoadAsync();
    Task SaveAsync();
}

public class SettingsService : ISettingsService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;
    
    public AppSettings Current { get; private set; } = new();

    public SettingsService(string filePath = "settings.json")
    {
        _filePath = filePath;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
        {
            Current = new AppSettings();
            await SaveAsync();
            return;
        }

        using var stream = File.OpenRead(_filePath);
        Current = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _options) 
                  ?? new AppSettings();
    }

    public async Task SaveAsync()
    {
        using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, Current, _options);
    }
}
