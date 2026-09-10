using System.Text.RegularExpressions;

namespace Teleprompter.Services.Transposition;

public interface IChromaticTranspositionService
{
    string Transpose(string chord, int semitones);
}

public class ChromaticTranspositionService : IChromaticTranspositionService
{
    private static readonly string[] AngloNotes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private static readonly string[] LatinaNotes = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" };

    // Regex to match a note at the beginning, followed by extensions, and optionally a bass note
    // Anglo: [A-G][b#]?
    // Latina: (Do|Re|Mi|Fa|Sol|La|Si)[b#]?
    private static readonly Regex ChordRegex = new(
        @"^(?<root>(?:Do|Re|Mi|Fa|Sol|La|Si|[A-G])[b#]?)(?<ext>[^\/]*)?(?:\/(?<bass>(?:Do|Re|Mi|Fa|Sol|La|Si|[A-G])[b#]?))?$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Transpose(string chord, int semitones)
    {
        if (string.IsNullOrWhiteSpace(chord)) return chord;

        var match = ChordRegex.Match(chord);
        if (!match.Success) return chord; // If it doesn't look like a chord, return as is.

        string root = match.Groups["root"].Value;
        string ext = match.Groups["ext"].Value;
        string bass = match.Groups["bass"].Value;

        bool isLatina = IsLatina(root);

        string transposedRoot = TransposeNote(root, semitones, isLatina);
        
        if (string.IsNullOrEmpty(bass))
        {
            return $"{transposedRoot}{ext}";
        }

        string transposedBass = TransposeNote(bass, semitones, isLatina);
        return $"{transposedRoot}{ext}/{transposedBass}";
    }

    private bool IsLatina(string note)
    {
        note = note.Replace("b", "").Replace("#", "");
        return note.Equals("Do", StringComparison.OrdinalIgnoreCase) ||
               note.Equals("Re", StringComparison.OrdinalIgnoreCase) ||
               note.Equals("Mi", StringComparison.OrdinalIgnoreCase) ||
               note.Equals("Fa", StringComparison.OrdinalIgnoreCase) ||
               note.Equals("Sol", StringComparison.OrdinalIgnoreCase) ||
               note.Equals("La", StringComparison.OrdinalIgnoreCase) ||
               note.Equals("Si", StringComparison.OrdinalIgnoreCase);
    }

    private string TransposeNote(string note, int semitones, bool outputLatina)
    {
        int index = GetNoteIndex(note);
        if (index == -1) return note; // fallback

        // Calculate new index with positive modulo
        int newIndex = (index + semitones) % 12;
        if (newIndex < 0) newIndex += 12;

        return outputLatina ? LatinaNotes[newIndex] : AngloNotes[newIndex];
    }

    private int GetNoteIndex(string note)
    {
        // Normalize
        note = char.ToUpper(note[0]) + note.Substring(1).ToLower();

        // Handle flats by converting to previous sharp
        if (note.EndsWith("b"))
        {
            // Find the natural note index
            string natural = note.Substring(0, note.Length - 1);
            int naturalIndex = IndexOfNote(natural);
            if (naturalIndex == -1) return -1;
            
            int flatIndex = (naturalIndex - 1) % 12;
            if (flatIndex < 0) flatIndex += 12;
            return flatIndex;
        }

        return IndexOfNote(note);
    }

    private int IndexOfNote(string note)
    {
        for (int i = 0; i < AngloNotes.Length; i++)
        {
            if (AngloNotes[i].Equals(note, StringComparison.OrdinalIgnoreCase)) return i;
        }
        for (int i = 0; i < LatinaNotes.Length; i++)
        {
            if (LatinaNotes[i].Equals(note, StringComparison.OrdinalIgnoreCase)) return i;
        }
        return -1;
    }
}
