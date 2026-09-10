using CommunityToolkit.Mvvm.ComponentModel;
using Teleprompter.Services.Settings;
using Teleprompter.Data.Context;

namespace Teleprompter.Wpf.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private string _windowTitle = "Teleprompter 2.0 - Studio";

    [ObservableProperty]
    private string _currentTheme = "Dark";

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
    }
}
