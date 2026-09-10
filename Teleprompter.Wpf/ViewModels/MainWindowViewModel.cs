using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Teleprompter.Core.Models;
using Teleprompter.Data.Context;
using Teleprompter.Services.Settings;

namespace Teleprompter.Wpf.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private string _windowTitle = "Teleprompter 2.0 - Studio";

    [ObservableProperty]
    private string _currentTheme = "Dark";

    [ObservableProperty]
    private ObservableCollection<Song> _songs = new();

    [ObservableProperty]
    private ObservableCollection<Folder> _folders = new();

    [ObservableProperty]
    private Song? _selectedSong;

    public MainWindowViewModel(ISettingsService settingsService, AppDbContext dbContext)
    {
        _settingsService = settingsService;
        _dbContext = dbContext;
        
        LoadInitialData();
    }

    private async void LoadInitialData()
    {
        await _settingsService.LoadAsync();
        CurrentTheme = _settingsService.Current.Theme;

        // Load data from DB
        var songs = await _dbContext.Songs.ToListAsync();
        var folders = await _dbContext.Folders.Include(f => f.SongFolders).ThenInclude(sf => sf.Song).ToListAsync();

        Songs = new ObservableCollection<Song>(songs);
        Folders = new ObservableCollection<Folder>(folders);
    }
}
