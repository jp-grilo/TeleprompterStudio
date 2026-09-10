namespace Teleprompter.Core.Models;

public class Song
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public int TransposeAmount { get; set; } = 0;
    public double ScrollSpeed { get; set; } = 1.0;

    public ICollection<SongFolder> SongFolders { get; set; } = new List<SongFolder>();
}

public class Folder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public ICollection<SongFolder> SongFolders { get; set; } = new List<SongFolder>();
}

public class SongFolder
{
    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public int FolderId { get; set; }
    public Folder Folder { get; set; } = null!;

    public int Order { get; set; } = 0;
}
