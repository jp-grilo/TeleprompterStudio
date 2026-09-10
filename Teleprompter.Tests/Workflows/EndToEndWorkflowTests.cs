using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Teleprompter.Core.Models;
using Teleprompter.Data.Context;
using Teleprompter.Services.Packages;
using Teleprompter.Services.Parsing;
using Teleprompter.Services.Scraping;
using Teleprompter.Services.Transposition;
using Xunit;

namespace Teleprompter.Tests.Workflows;

public class EndToEndWorkflowTests : IDisposable
{
    private readonly SqliteConnection _dbConnection;
    private readonly AppDbContext _dbContext;

    public EndToEndWorkflowTests()
    {
        _dbConnection = new SqliteConnection("DataSource=:memory:");
        _dbConnection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_dbConnection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task CompleteWorkflow_Scrape_Parse_Transpose_Database_And_Json()
    {
        // 1. Iniciar Serviços
        var scraper = new CifraClubScraper();
        var parser = new UniversalChordParserService();
        var transposer = new ChromaticTranspositionService();
        var jsonService = new JsonSongPackageService();

        // 2. CASO 1: Cifra Real via Web Scraping
        var url = "https://www.cifraclub.com.br/chitaozinho-e-xororo/evidencias/";
        var rawWebText = await scraper.ScrapeAsync(url);
        var parsedWebSong = parser.Parse(rawWebText);

        // Validar que o parser encontrou algo real e estruturado
        parsedWebSong.Sections.Should().NotBeEmpty();
        string firstWebSectionContent = parsedWebSong.Sections.First(s => s.Content.Contains("[")).Content;
        
        // 3. Transpor um acorde do conteúdo raspado (Prova de conceito)
        // Se encontramos um [E], transpor +2 vira [F#]
        string transposedLine = transposer.Transpose("E", 2);
        transposedLine.Should().Be("F#");

        // 4. CASO 2: Música Manual
        string manualText = "Refrão:\nC          G\nEssa é uma música manual\nAm         F\nCriada no teste";
        var parsedManualSong = parser.Parse(manualText);
        parsedManualSong.Sections.Count.Should().Be(1);
        parsedManualSong.Sections[0].Header.Should().Be("Refrão");
        parsedManualSong.Sections[0].Content.Should().Contain("[C]");

        // 5. Salvar no Banco de Dados SQLite
        var dbSong1 = new Song 
        { 
            Title = "Evidências", 
            Artist = "Chitãozinho & Xororó", 
            RawContent = rawWebText,
            TransposeAmount = 2
        };
        var dbSong2 = new Song
        {
            Title = "Música de Teste",
            Artist = "Eu Mesmo",
            RawContent = manualText,
            TransposeAmount = 0
        };

        _dbContext.Songs.Add(dbSong1);
        _dbContext.Songs.Add(dbSong2);
        await _dbContext.SaveChangesAsync();

        var savedSongs = await _dbContext.Songs.ToListAsync();
        savedSongs.Should().HaveCount(2);

        // 6. Exportar para JSON (Funcionalidade de Backup)
        var package = new SongPackage
        {
            Title = dbSong1.Title,
            Artist = dbSong1.Artist,
            SongData = parsedWebSong,
            TransposeAmount = dbSong1.TransposeAmount
        };

        string exportedJson = jsonService.Export(package);
        exportedJson.Should().Contain("Evidências");
        exportedJson.Should().Contain("TransposeAmount");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _dbConnection.Close();
    }
}
