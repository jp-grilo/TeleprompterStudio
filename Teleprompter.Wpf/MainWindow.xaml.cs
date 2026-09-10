using System.Windows;
using Teleprompter.Core.Models;
using Teleprompter.Wpf.ViewModels;

namespace Teleprompter.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainWindowViewModel vm && e.NewValue is TreeItem item)
        {
            if (item.Tag is Song song)
            {
                vm.SelectedSong = song;
            }
            // Se clicar numa pasta, não faz nada com a música atual para não piscar a tela
        }
    }

    private void OpenStage_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.SelectedSong != null)
        {
            var stage = new TeleprompterWindow(vm.SelectedSong);
            stage.Show();
        }
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // To be implemented fully in ViewModel, filtering logic
    }

    private void ContextMenu_Stage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement el && el.DataContext is TreeItem item && item.Tag is Song song)
        {
            var stage = new TeleprompterWindow(song);
            stage.Show();
        }
    }

    private void ContextMenu_Favorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement el && el.DataContext is TreeItem item && item.Tag is Song song)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ToggleFavoriteCommand.Execute(song);
            }
        }
    }
}