using Microsoft.EntityFrameworkCore;
using Teleprompter.Core.Models;

namespace Teleprompter.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<SongFolder> SongFolders => Set<SongFolder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many setup
        modelBuilder.Entity<SongFolder>()
            .HasKey(sf => new { sf.SongId, sf.FolderId });

        modelBuilder.Entity<SongFolder>()
            .HasOne(sf => sf.Song)
            .WithMany(s => s.SongFolders)
            .HasForeignKey(sf => sf.SongId);

        modelBuilder.Entity<SongFolder>()
            .HasOne(sf => sf.Folder)
            .WithMany(f => f.SongFolders)
            .HasForeignKey(sf => sf.FolderId);
    }
}
