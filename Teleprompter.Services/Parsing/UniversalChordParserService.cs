using System.Text;
using System.Text.RegularExpressions;

namespace Teleprompter.Services.Parsing;

public class UniversalChordParserService : IUniversalChordParserService
{
    private static readonly Regex SectionHeaderRegex = new(@"^\[?(Verse|Chorus|Intro|Outro|Bridge|Solo|Interlude|Pre-Chorus|Part|Refr[ãa]o)[^\]\n]*\]?:?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Very basic regex to identify a line that only contains chords and spaces
    private static readonly Regex ChordLineRegex = new(@"^(?:\s*[A-G](?:#|b)?(?:m|maj|min|aug|dim|sus|add|\d)*[\+\-\(]?\d*[\)]?(?:\/[A-G](?:#|b)?)?\s*)+$", RegexOptions.Compiled);

    public ParsedSong Parse(string input)
    {
        var song = new ParsedSong();
        if (string.IsNullOrWhiteSpace(input)) return song;

        var lines = input.Replace("\r", "").Split('\n');
        
        SongSection currentSection = new SongSection();
        song.Sections.Add(currentSection);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                if (!string.IsNullOrWhiteSpace(currentSection.Content))
                    currentSection.Content += "\n";
                continue;
            }

            // Check if header
            if (SectionHeaderRegex.IsMatch(trimmedLine))
            {
                string header = trimmedLine.Trim('[', ']', ':').Trim();
                
                if (string.IsNullOrWhiteSpace(currentSection.Content) && string.IsNullOrWhiteSpace(currentSection.Header))
                {
                    currentSection.Header = header;
                }
                else
                {
                    currentSection = new SongSection { Header = header };
                    song.Sections.Add(currentSection);
                }
                continue;
            }

            // Check if it's a chord-only line and next line is lyrics (not empty, not chords)
            if (ChordLineRegex.IsMatch(line) && !line.Contains("["))
            {
                if (i + 1 < lines.Length)
                {
                    var nextLine = lines[i + 1];
                    if (!string.IsNullOrWhiteSpace(nextLine) && !ChordLineRegex.IsMatch(nextLine) && !SectionHeaderRegex.IsMatch(nextLine.Trim()))
                    {
                        // Merge them
                        string merged = MergeChordsAndLyrics(line, nextLine);
                        currentSection.Content += merged + "\n";
                        i++; // skip lyric line
                        continue;
                    }
                }
                // If we couldn't merge, just add it as ChordPro line
                currentSection.Content += MergeChordsAndLyrics(line, "") + "\n";
                continue;
            }

            currentSection.Content += line + "\n";
        }

        // Cleanup trailing newlines
        foreach (var sec in song.Sections)
        {
            sec.Content = sec.Content.TrimEnd('\n');
        }

        // Remove empty sections
        song.Sections.RemoveAll(s => string.IsNullOrWhiteSpace(s.Header) && string.IsNullOrWhiteSpace(s.Content));

        return song;
    }

    private string MergeChordsAndLyrics(string chordLine, string lyricLine)
    {
        var sb = new StringBuilder();
        int lyricIndex = 0;

        // Find all chords in the chord line
        var matches = Regex.Matches(chordLine, @"\S+");
        
        int lastChordEnd = 0;
        foreach (Match match in matches)
        {
            int chordPos = match.Index;
            string chord = match.Value;

            // Append lyrics up to this chord position
            while (lyricIndex < chordPos && lyricIndex < lyricLine.Length)
            {
                sb.Append(lyricLine[lyricIndex]);
                lyricIndex++;
            }

            // If lyric line is shorter than chord pos, pad with spaces (optional, but keeps alignment)
            while (lyricIndex < chordPos)
            {
                sb.Append(" ");
                lyricIndex++;
            }

            sb.Append($"[{chord}]");
        }

        // Append remaining lyrics
        if (lyricIndex < lyricLine.Length)
        {
            sb.Append(lyricLine.Substring(lyricIndex));
        }

        return sb.ToString();
    }
}
