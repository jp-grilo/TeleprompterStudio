namespace Teleprompter.Services.Parsing;

public class ParsedSong
{
    public List<SongSection> Sections { get; set; } = new();
}

public class SongSection
{
    public string Header { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public interface IUniversalChordParserService
{
    ParsedSong Parse(string input);
}
