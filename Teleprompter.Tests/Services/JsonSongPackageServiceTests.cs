using FluentAssertions;
using Teleprompter.Services.Packages;
using Teleprompter.Services.Parsing;
using Xunit;

namespace Teleprompter.Tests.Services;

public class JsonSongPackageServiceTests
{
    private readonly JsonSongPackageService _sut;

    public JsonSongPackageServiceTests()
    {
        _sut = new JsonSongPackageService();
    }

    [Fact]
    public void ExportImport_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new SongPackage
        {
            Title = "Test Song",
            Artist = "Test Artist",
            TransposeAmount = 2,
            ScrollSpeed = 1.5,
            SongData = new ParsedSong
            {
                Sections = new List<SongSection>
                {
                    new SongSection { Header = "Verse 1", Content = "[C]Hello" }
                }
            }
        };

        // Act
        var json = _sut.Export(original);
        var imported = _sut.Import(json);

        // Assert
        imported.Should().NotBeNull();
        imported.Id.Should().Be(original.Id);
        imported.Title.Should().Be("Test Song");
        imported.Artist.Should().Be("Test Artist");
        imported.TransposeAmount.Should().Be(2);
        imported.ScrollSpeed.Should().Be(1.5);
        imported.SongData.Sections.Should().HaveCount(1);
        imported.SongData.Sections[0].Header.Should().Be("Verse 1");
        imported.SongData.Sections[0].Content.Should().Be("[C]Hello");
    }

    [Fact]
    public async Task ExportImportToFile_RoundTrip_PreservesData()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var original = new SongPackage
        {
            Title = "File Song",
            TransposeAmount = -1
        };

        try
        {
            // Act
            await _sut.ExportToFileAsync(original, tempFile);
            var imported = await _sut.ImportFromFileAsync(tempFile);

            // Assert
            imported.Title.Should().Be("File Song");
            imported.TransposeAmount.Should().Be(-1);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
