using System.Windows;
using System.Windows.Input;
using Teleprompter.Core.Models;

namespace Teleprompter.Wpf;

public partial class TeleprompterWindow : Window
{
    public TeleprompterWindow(Song song)
    {
        InitializeComponent();
        TitleBlock.Text = song.Title;
        ContentBlock.Text = song.RawContent; // Por enquanto exibimos raw, no futuro passaremos pelo Parser e Transposer
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close();
        }
        else if (e.Key == Key.Space || e.Key == Key.PageDown)
        {
            StageScroll.ScrollToVerticalOffset(StageScroll.VerticalOffset + 100);
        }
        else if (e.Key == Key.PageUp)
        {
            StageScroll.ScrollToVerticalOffset(StageScroll.VerticalOffset - 100);
        }
    }
}
