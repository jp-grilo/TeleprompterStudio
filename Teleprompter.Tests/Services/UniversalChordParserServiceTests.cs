using FluentAssertions;
using Teleprompter.Services.Parsing;
using Xunit;

namespace Teleprompter.Tests.Services;

public class UniversalChordParserServiceTests
{
    private readonly UniversalChordParserService _sut;

    public UniversalChordParserServiceTests()
    {
        _sut = new UniversalChordParserService();
    }

    [Fact]
    public void Parse_ChordProFormat_ExtractsSectionsAndKeepsContent()
    {
        string input = "[Verse 1]\n[C]Hello [G]world\n[Am]It is a [F]test\n\n[Chorus]\n[C]Oh [G]yeah";
        
        var result = _sut.Parse(input);

        result.Sections.Should().HaveCount(2);
        result.Sections[0].Header.Should().Be("Verse 1");
        result.Sections[0].Content.Trim().Should().Be("[C]Hello [G]world\n[Am]It is a [F]test");
        result.Sections[1].Header.Should().Be("Chorus");
        result.Sections[1].Content.Trim().Should().Be("[C]Oh [G]yeah");
    }

    [Fact]
    public void Parse_ChordsOverLyrics_ConvertsToChordPro()
    {
        string input = "Verse 1:\nC        G\nHello world\nAm        F\nIt is a test";
        
        var result = _sut.Parse(input);

        result.Sections.Should().HaveCount(1);
        result.Sections[0].Header.Should().Be("Verse 1");
        result.Sections[0].Content.Trim().Should().Be("[C]Hello wor[G]ld\n[Am]It is a te[F]st"); 
    }

    [Fact]
    public void Parse_ChordsOverLyrics_Simple_ConvertsToChordPro()
    {
        string input = "C   G\nHi  There";
        // C at 0, G at 4.
        // Hi at 0 (len 2). There at 4.
        // Result: "[C]Hi  [G]There"
        var result = _sut.Parse(input);
        
        result.Sections.Should().HaveCount(1);
        result.Sections[0].Header.Should().BeEmpty();
        result.Sections[0].Content.Trim().Should().Be("[C]Hi  [G]There");
    }
}
