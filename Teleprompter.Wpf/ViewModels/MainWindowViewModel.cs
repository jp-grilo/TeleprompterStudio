using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Teleprompter.Core.Models;
using Teleprompter.Data.Context;
using Teleprompter.Services.Settings;

namespace Teleprompter.Wpf.ViewModels;

public class TreeItem
{
    public string Header { get; set; } = string.Empty;
    public object? Tag { get; set; }
    public bool IsSystemFolder { get; set; } = false;
    public bool IsUserFolder { get; set; } = false;
    public bool IsSong { get; set; } = false;
    public ObservableCollection<TreeItem> Children { get; set; } = new();
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private string _windowTitle = "Teleprompter 2.0 - Studio";

    [ObservableProperty]
    private string _currentTheme = "Dark";

    [ObservableProperty]
    private ObservableCollection<TreeItem> _treeNodes = new();

    [ObservableProperty]
    private Song? _selectedSong;

    [ObservableProperty]
    private bool _isEditMode = false;

    // Campos temporários para caso o usuário cancele a edição
    private string _backupRawContent = string.Empty;
    private string _backupTitle = string.Empty;
    private string _backupArtist = string.Empty;

    public MainWindowViewModel(ISettingsService settingsService, AppDbContext dbContext)
    {
        _settingsService = settingsService;
        _dbContext = dbContext;
        
        LoadInitialData();
    }

    public async void LoadInitialData()
    {
        await _settingsService.LoadAsync();
        CurrentTheme = _settingsService.Current.Theme;

        await BuildTreeAsync();
    }

    partial void OnSelectedSongChanged(Song? value)
    {
        IsEditMode = false;
    }

    [RelayCommand]
    private void EnterEditMode()
    {
        if (SelectedSong != null)
        {
            _backupRawContent = SelectedSong.RawContent;
            _backupTitle = SelectedSong.Title;
            _backupArtist = SelectedSong.Artist;
            IsEditMode = true;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (SelectedSong != null)
        {
            SelectedSong.RawContent = _backupRawContent;
            SelectedSong.Title = _backupTitle;
            SelectedSong.Artist = _backupArtist;
            
            // Força a UI a atualizar as propriedades
            OnPropertyChanged(nameof(SelectedSong));
            IsEditMode = false;
        }
    }

    [RelayCommand]
    private async Task SaveSongAsync()
    {
        if (SelectedSong != null)
        {
            _dbContext.Songs.Update(SelectedSong);
            await _dbContext.SaveChangesAsync();
            IsEditMode = false;
            await BuildTreeAsync(); // Atualiza árvore caso o nome/artista tenha mudado
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(Song? song)
    {
        var target = song ?? SelectedSong;
        if (target != null)
        {
            target.IsFavorite = !target.IsFavorite;
            _dbContext.Songs.Update(target);
            await _dbContext.SaveChangesAsync();
            await BuildTreeAsync(); // Recarrega para mostrar/esconder na pasta Favoritos
        }
    }

    public async Task BuildTreeAsync()
    {
        var songs = await _dbContext.Songs.OrderBy(s => s.Title).ToListAsync();
        var folders = await _dbContext.Folders.Include(f => f.SongFolders).ThenInclude(sf => sf.Song).ToListAsync();

        var nodes = new ObservableCollection<TreeItem>();

        // 1. Todas as Músicas (A-Z)
        var allSongsNode = new TreeItem { Header = "Todas as Músicas", IsSystemFolder = true };
        foreach (var song in songs)
        {
            allSongsNode.Children.Add(new TreeItem { Header = song.Title, Tag = song, IsSong = true });
        }
        nodes.Add(allSongsNode);

        // 2. Favoritas
        var favoritesNode = new TreeItem { Header = "⭐ Favoritas", IsSystemFolder = true };
        foreach (var song in songs.Where(s => s.IsFavorite))
        {
            favoritesNode.Children.Add(new TreeItem { Header = song.Title, Tag = song, IsSong = true });
        }
        nodes.Add(favoritesNode);

        // 3. Artistas
        var artistsNode = new TreeItem { Header = "Artistas", IsSystemFolder = true };
        var groupedByArtist = songs.GroupBy(s => string.IsNullOrWhiteSpace(s.Artist) ? "Desconhecido" : s.Artist).OrderBy(g => g.Key);
        
        foreach (var artistGroup in groupedByArtist)
        {
            var artistItem = new TreeItem { Header = artistGroup.Key, IsSystemFolder = true };
            foreach (var song in artistGroup.OrderBy(s => s.Title))
            {
                artistItem.Children.Add(new TreeItem { Header = song.Title, Tag = song, IsSong = true });
            }
            artistsNode.Children.Add(artistItem);
        }
        nodes.Add(artistsNode);

        // 4. Pastas Customizadas do Usuário (Root)
        var rootFolders = folders.Where(f => f.ParentFolderId == null).OrderBy(f => f.OrderIndex);
        foreach (var rootFolder in rootFolders)
        {
            nodes.Add(BuildFolderTree(rootFolder, folders));
        }

        TreeNodes = nodes;
    }

    private TreeItem BuildFolderTree(Folder folder, List<Folder> allFolders)
    {
        var item = new TreeItem { Header = folder.Name, Tag = folder, IsUserFolder = true };
        
        // Adiciona subpastas recursivamente
        var subFolders = allFolders.Where(f => f.ParentFolderId == folder.Id).OrderBy(f => f.OrderIndex);
        foreach (var sub in subFolders)
        {
            item.Children.Add(BuildFolderTree(sub, allFolders));
        }

        // Adiciona as músicas desta pasta
        foreach (var sf in folder.SongFolders.OrderBy(x => x.Order))
        {
            item.Children.Add(new TreeItem { Header = sf.Song.Title, Tag = sf.Song, IsSong = true });
        }

        return item;
    }
}
