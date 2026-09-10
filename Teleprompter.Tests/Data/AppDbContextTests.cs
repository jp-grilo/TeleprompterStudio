using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Teleprompter.Core.Models;
using Teleprompter.Data.Context;
using Xunit;

namespace Teleprompter.Tests.Data;

public class AppDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public AppDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CrudOperations_Async_WorkAsExpected()
    {
        // 1. CREATE
        using (var context = new AppDbContext(_options))
        {
            var song = new Song { Title = "Yellow", Artist = "Coldplay" };
            var folder = new Folder { Name = "Repertório Rock" };
            
            context.Songs.Add(song);
            context.Folders.Add(folder);
            await context.SaveChangesAsync();

            context.SongFolders.Add(new SongFolder { SongId = song.Id, FolderId = folder.Id, Order = 1 });
            await context.SaveChangesAsync();
        }

        // 2. READ
        using (var context = new AppDbContext(_options))
        {
            var song = await context.Songs
                .Include(s => s.SongFolders)
                .ThenInclude(sf => sf.Folder)
                .FirstOrDefaultAsync(s => s.Title == "Yellow");

            song.Should().NotBeNull();
            song!.Artist.Should().Be("Coldplay");
            song.SongFolders.Should().HaveCount(1);
            song.SongFolders.First().Folder.Name.Should().Be("Repertório Rock");

            // 3. UPDATE
            song.TransposeAmount = 2;
            context.Songs.Update(song);
            await context.SaveChangesAsync();
        }

        // 4. DELETE
        using (var context = new AppDbContext(_options))
        {
            var folder = await context.Folders.FirstAsync();
            context.Folders.Remove(folder); // Deverá remover também da tabela de junção em cascata
            await context.SaveChangesAsync();

            var relations = await context.SongFolders.ToListAsync();
            relations.Should().BeEmpty();
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }
}
