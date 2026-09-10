using FluentAssertions;
using Teleprompter.Services.Settings;
using Xunit;

namespace Teleprompter.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testFile;
    private readonly SettingsService _sut;

    public SettingsServiceTests()
    {
        _testFile = Path.GetTempFileName();
        _sut = new SettingsService(_testFile);
    }

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_CreatesDefaultFile()
    {
        // Limpar o arquivo temporário criado no construtor para simular não existência
        File.Delete(_testFile);

        await _sut.LoadAsync();

        File.Exists(_testFile).Should().BeTrue();
        _sut.Current.Should().NotBeNull();
        _sut.Current.Theme.Should().Be("Dark"); // Default value
    }

    [Fact]
    public async Task SaveAsync_ModifiesSettings_LoadsCorrectly()
    {
        await _sut.LoadAsync();
        
        _sut.Current.Theme = "Light";
        _sut.Current.DefaultNotation = "Latina";
        
        await _sut.SaveAsync();

        var novoSut = new SettingsService(_testFile);
        await novoSut.LoadAsync();

        novoSut.Current.Theme.Should().Be("Light");
        novoSut.Current.DefaultNotation.Should().Be("Latina");
    }

    public void Dispose()
    {
        if (File.Exists(_testFile))
        {
            File.Delete(_testFile);
        }
    }
}
