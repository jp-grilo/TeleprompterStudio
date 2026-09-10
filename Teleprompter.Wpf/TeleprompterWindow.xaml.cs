using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Teleprompter.Core.Models;
using Teleprompter.Services.Parsing;
using Teleprompter.Services.Transposition;

namespace Teleprompter.Wpf;

public partial class TeleprompterWindow : Window
{
    private readonly Song _song;
    private readonly IUniversalChordParserService _parser;
    private readonly IChromaticTranspositionService _transposer;
    private DispatcherTimer? _scrollTimer;
    private bool _isAutoScrolling = false;

    public TeleprompterWindow(Song song)
    {
        InitializeComponent();
        _song = song;
        
        // Resolvendo serviços via Injeção de Dependência da Aplicação
        _parser = ((App)Application.Current).ServiceProvider.GetRequiredService<IUniversalChordParserService>();
        _transposer = ((App)Application.Current).ServiceProvider.GetRequiredService<IChromaticTranspositionService>();

        TitleBlock.Text = song.Title;
        
        RenderSong();
        SetupScrollEngine();
    }

    private void RenderSong()
    {
        // 1. Parsear texto bruto para seções
        var parsedSong = _parser.Parse(_song.RawContent);

        // Limpar placeholder inicial
        LinesContainer.Children.Clear();
        LinesContainer.Children.Add(TitleBlock); // recoloca o título

        foreach (var section in parsedSong.Sections)
        {
            // Título da Seção (Refrão, Verso, etc)
            if (!string.IsNullOrWhiteSpace(section.Header))
            {
                LinesContainer.Children.Add(new TextBlock
                {
                    Text = section.Header.ToUpper(),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC")),
                    FontSize = 36,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 30, 0, 10)
                });
            }

            // Renderizar as linhas intercaladas
            var lines = section.Content.Split('\n');
            foreach (var line in lines)
            {
                RenderIntercalatedLine(line);
            }
        }
    }

    private void RenderIntercalatedLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            LinesContainer.Children.Add(new TextBlock { Height = 20 }); // Espaçamento
            return;
        }

        // Se a linha não tiver acordes (sem colchetes), renderiza texto puro
        if (!line.Contains("["))
        {
            LinesContainer.Children.Add(new TextBlock
            {
                Text = line.TrimEnd(),
                Foreground = Brushes.White,
                FontSize = 48,
                FontFamily = new FontFamily("Consolas")
            });
            return;
        }

        // Criar WrapPanel para suportar o intercalamento (Acorde em cima, Palavra embaixo) horizontalmente
        var linePanel = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 10) };

        string remaining = line;
        while (remaining.Length > 0)
        {
            int nextBracket = remaining.IndexOf('[');
            
            // Texto puro antes do próximo acorde
            if (nextBracket > 0)
            {
                string textPart = remaining.Substring(0, nextBracket);
                linePanel.Children.Add(CreateChordSyllableBlock("", textPart));
                remaining = remaining.Substring(nextBracket);
            }
            else if (nextBracket == 0)
            {
                int endBracket = remaining.IndexOf(']');
                if (endBracket > 0)
                {
                    string chordRaw = remaining.Substring(1, endBracket - 1);
                    string chordTransposed = _transposer.Transpose(chordRaw, _song.TransposeAmount);
                    remaining = remaining.Substring(endBracket + 1);

                    // A palavra atrelada ao acorde (até o próximo acorde ou fim)
                    int nextNextBracket = remaining.IndexOf('[');
                    string syllable = nextNextBracket >= 0 ? remaining.Substring(0, nextNextBracket) : remaining;
                    
                    linePanel.Children.Add(CreateChordSyllableBlock(chordTransposed, syllable));
                    
                    remaining = nextNextBracket >= 0 ? remaining.Substring(nextNextBracket) : "";
                }
                else
                {
                    // Fallback se colchete quebrado
                    linePanel.Children.Add(CreateChordSyllableBlock("", remaining));
                    break;
                }
            }
            else
            {
                // Sem mais acordes
                linePanel.Children.Add(CreateChordSyllableBlock("", remaining));
                break;
            }
        }

        LinesContainer.Children.Add(linePanel);
    }

    private StackPanel CreateChordSyllableBlock(string chord, string text)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 0, 0) };
        
        // Acorde
        panel.Children.Add(new TextBlock 
        { 
            Text = chord, 
            Foreground = Brushes.Yellow, 
            FontSize = 38, 
            FontWeight = FontWeights.Bold, 
            FontFamily = new FontFamily("Consolas"),
            Height = string.IsNullOrEmpty(chord) ? 40 : double.NaN // Mantém altura constante se vazio
        });

        // Letra
        // Converter espaços em non-breaking spaces para preservar alinhamento no WrapPanel se necessário, mas TextBlock já lida com trailing spaces se configurado.
        panel.Children.Add(new TextBlock 
        { 
            Text = text.Replace(" ", "\u00A0"), 
            Foreground = Brushes.White, 
            FontSize = 48, 
            FontFamily = new FontFamily("Consolas") 
        });

        return panel;
    }

    private void SetupScrollEngine()
    {
        _scrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60fps
        _scrollTimer.Tick += (s, e) =>
        {
            if (_isAutoScrolling)
            {
                StageScroll.ScrollToVerticalOffset(StageScroll.VerticalOffset + _song.ScrollSpeed);
            }
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close();
        }
        else if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            // Play/Pause do Scroll Automático
            _isAutoScrolling = !_isAutoScrolling;
            if (_isAutoScrolling) _scrollTimer?.Start(); else _scrollTimer?.Stop();
        }
        else if (e.Key == Key.PageDown || e.Key == Key.Down)
        {
            StageScroll.ScrollToVerticalOffset(StageScroll.VerticalOffset + 50);
        }
        else if (e.Key == Key.PageUp || e.Key == Key.Up)
        {
            StageScroll.ScrollToVerticalOffset(StageScroll.VerticalOffset - 50);
        }
    }
}
