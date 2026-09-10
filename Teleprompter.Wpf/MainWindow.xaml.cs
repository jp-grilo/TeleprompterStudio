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
            else
            {
                vm.SelectedSong = null;
            }
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
}