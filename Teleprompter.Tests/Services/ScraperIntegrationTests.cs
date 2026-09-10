using FluentAssertions;
using Teleprompter.Services.Parsing;
using Teleprompter.Services.Scraping;
using Xunit;

namespace Teleprompter.Tests.Services;

public class ScraperIntegrationTests
{
    [Fact]
    public async Task ScrapeAndParse_CifraClubUrl_ReturnsParsedChordPro()
    {
        // 1. Arrange
        var scraper = new CifraClubScraper();
        var parser = new UniversalChordParserService();
        var url = "https://www.cifraclub.com.br/legiao-urbana/tempo-perdido/";

        // 2. Act
        var rawText = await scraper.ScrapeAsync(url);
        var parsedSong = parser.Parse(rawText);

        // 3. Assert
        rawText.Should().NotBeNullOrWhiteSpace();
        parsedSong.Should().NotBeNull();
        
        // A cifra do tempo perdido começa com o solo e tem seções.
        // Pelo menos deve ter extraído algumas seções (ex: Intro, Primeira Parte, Refrão)
        parsedSong.Sections.Should().NotBeEmpty();
        
        // O conteúdo da primeira ou segunda seção deve ter marcações em ChordPro (colchetes)
        bool hasChordProMarks = parsedSong.Sections.Any(s => s.Content.Contains("[C]") || s.Content.Contains("[Am]") || s.Content.Contains("[Em]"));
        hasChordProMarks.Should().BeTrue("O parser deve ter convertido as cifras sobre as letras para o formato ChordPro [Acorde]");
    }
}
