using System.Text.Json;
using Teleprompter.Services.Parsing;

namespace Teleprompter.Services.Packages;

public class SongPackage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public ParsedSong SongData { get; set; } = new();
    
    // Overrides
    public int TransposeAmount { get; set; } = 0;
    public double ScrollSpeed { get; set; } = 1.0;
}

public interface IJsonSongPackageService
{
    string Export(SongPackage package);
    SongPackage Import(string json);
    Task ExportToFileAsync(SongPackage package, string filePath);
    Task<SongPackage> ImportFromFileAsync(string filePath);
}

public class JsonSongPackageService : IJsonSongPackageService
{
    private readonly JsonSerializerOptions _options;

    public JsonSongPackageService()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public string Export(SongPackage package)
    {
        return JsonSerializer.Serialize(package, _options);
    }

    public SongPackage Import(string json)
    {
        return JsonSerializer.Deserialize<SongPackage>(json, _options) 
               ?? throw new InvalidOperationException("Failed to deserialize the JSON string into a SongPackage.");
    }

    public async Task ExportToFileAsync(SongPackage package, string filePath)
    {
        using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, package, _options);
    }

    public async Task<SongPackage> ImportFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Song package file not found.", filePath);

        using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<SongPackage>(stream, _options)
               ?? throw new InvalidOperationException("Failed to deserialize the JSON file into a SongPackage.");
    }
}
