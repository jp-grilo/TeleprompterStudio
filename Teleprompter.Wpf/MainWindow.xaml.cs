using System.Windows;
using Teleprompter.Wpf.ViewModels;

namespace Teleprompter.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}