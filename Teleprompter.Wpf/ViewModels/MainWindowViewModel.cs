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

        // 1. Favoritas
        var favoritesNode = new TreeItem { Header = "⭐ Favoritas" };
        foreach (var song in songs.Where(s => s.IsFavorite))
        {
            favoritesNode.Children.Add(new TreeItem { Header = song.Title, Tag = song });
        }
        nodes.Add(favoritesNode);

        // 2. Todas as Músicas (A-Z)
        var allSongsNode = new TreeItem { Header = "Todas as Músicas" };
        foreach (var song in songs)
        {
            allSongsNode.Children.Add(new TreeItem { Header = song.Title, Tag = song });
        }
        nodes.Add(allSongsNode);

        // 3. Artistas / Álbum / Música
        var artistsNode = new TreeItem { Header = "Artistas", Children = new ObservableCollection<TreeItem>() };
        var groupedByArtist = songs.GroupBy(s => string.IsNullOrWhiteSpace(s.Artist) ? "Desconhecido" : s.Artist).OrderBy(g => g.Key);
        
        foreach (var artistGroup in groupedByArtist)
        {
            var artistItem = new TreeItem { Header = artistGroup.Key };
            var groupedByAlbum = artistGroup.GroupBy(s => string.IsNullOrWhiteSpace(s.Album) ? "Singles" : s.Album).OrderBy(g => g.Key);
            
            foreach (var albumGroup in groupedByAlbum)
            {
                var albumItem = new TreeItem { Header = albumGroup.Key };
                foreach (var song in albumGroup)
                {
                    albumItem.Children.Add(new TreeItem { Header = song.Title, Tag = song });
                }
                artistItem.Children.Add(albumItem);
            }
            artistsNode.Children.Add(artistItem);
        }
        nodes.Add(artistsNode);

        // 4. Minhas Músicas (Pastas Customizadas do Usuário)
        var repertoiresNode = new TreeItem { Header = "Minhas Músicas" };
        foreach (var folder in folders)
        {
            var folderItem = new TreeItem { Header = folder.Name };
            foreach (var sf in folder.SongFolders.OrderBy(x => x.Order))
            {
                folderItem.Children.Add(new TreeItem { Header = sf.Song.Title, Tag = sf.Song });
            }
            repertoiresNode.Children.Add(folderItem);
        }
        nodes.Add(repertoiresNode);

        TreeNodes = nodes;
    }
}
