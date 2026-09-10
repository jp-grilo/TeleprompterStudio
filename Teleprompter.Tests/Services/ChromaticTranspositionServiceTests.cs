using FluentAssertions;
using Teleprompter.Services.Transposition;
using Xunit;

namespace Teleprompter.Tests.Services;

public class ChromaticTranspositionServiceTests
{
    private readonly ChromaticTranspositionService _sut;

    public ChromaticTranspositionServiceTests()
    {
        _sut = new ChromaticTranspositionService();
    }

    [Theory]
    [InlineData("C", 2, "D")]
    [InlineData("G", 1, "G#")]
    [InlineData("B", 1, "C")]
    [InlineData("F#", -1, "F")]
    [InlineData("C#", -2, "B")]
    [InlineData("Ab", 2, "A#")] // Ab is G# (index 8). +2 = A# (index 10)
    public void Transpose_SimpleChords_ReturnsCorrectTransposition(string chord, int semitones, string expected)
    {
        var result = _sut.Transpose(chord, semitones);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("C/E", 2, "D/F#")]
    [InlineData("G/B", 1, "G#/C")]
    [InlineData("Am/G", -2, "Gm/F")]
    public void Transpose_InvertedBasses_ReturnsCorrectTransposition(string chord, int semitones, string expected)
    {
        var result = _sut.Transpose(chord, semitones);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Cmaj7", 2, "Dmaj7")]
    [InlineData("F#m7(b5)", 1, "Gm7(b5)")]
    [InlineData("Badd9", -1, "A#add9")]
    public void Transpose_ChordsWithExtensions_KeepsExtensions(string chord, int semitones, string expected)
    {
        var result = _sut.Transpose(chord, semitones);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Do", 2, "Re")]
    [InlineData("Sol", 1, "Sol#")]
    [InlineData("Si", 1, "Do")]
    [InlineData("Do/Mi", 2, "Re/Fa#")]
    [InlineData("Lam7", 2, "Sim7")]
    [InlineData("Mib", 2, "Fa")]
    public void Transpose_LatinaNotation_ReturnsLatinaTransposition(string chord, int semitones, string expected)
    {
        var result = _sut.Transpose(chord, semitones);
        result.Should().Be(expected);
    }
}
