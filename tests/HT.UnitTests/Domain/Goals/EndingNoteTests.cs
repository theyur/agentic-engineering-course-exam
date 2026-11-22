using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.UnitTests.Domain.Goals;

/// <summary>Slice 8 FR-007, HP-010: an ending note is trimmed, 1 to 500 code units, empty means none, and never printed.</summary>
public class EndingNoteTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Null_empty_or_blank_is_no_note(string? text)
    {
        Assert.True(EndingNote.TryCreate(text, out var note, out var tooLong));
        Assert.False(tooLong);
        Assert.Null(note);
    }

    [Fact]
    public void The_note_is_trimmed()
    {
        Assert.True(EndingNote.TryCreate("  Too busy.  ", out var note, out var tooLong));
        Assert.False(tooLong);
        Assert.Equal("Too busy.", note!.Value.Value);
    }

    [Fact]
    public void Five_hundred_code_units_are_accepted()
    {
        var text = new string('a', RecordLimits.MaxNoteLength);

        Assert.True(EndingNote.TryCreate(text, out var note, out var tooLong));
        Assert.False(tooLong);
        Assert.Equal(text, note!.Value.Value);
        Assert.Equal(500, RecordLimits.MaxNoteLength);
    }

    [Fact]
    public void Five_hundred_and_one_code_units_are_too_long()
    {
        Assert.False(EndingNote.TryCreate(new string('a', 501), out var note, out var tooLong));
        Assert.True(tooLong);
        Assert.Null(note);
    }

    [Fact]
    public void Five_hundred_and_one_with_surrounding_spaces_that_trim_to_500_are_accepted()
    {
        var text = new string('a', 500);

        Assert.True(EndingNote.TryCreate(" " + text, out var note, out var tooLong));
        Assert.False(tooLong);
        Assert.Equal(text, note!.Value.Value);
    }

    [Fact]
    public void A_surrogate_pair_counts_as_two_units()
    {
        // "😀" is one character written as two UTF-16 code units, so 250 of them are 500 units and one more is 502.
        var fits = string.Concat(Enumerable.Repeat("\U0001F600", 250));

        Assert.True(EndingNote.TryCreate(fits, out var note, out _));
        Assert.Equal(fits, note!.Value.Value);
        Assert.False(EndingNote.TryCreate(fits + "\U0001F600", out _, out var tooLong));
        Assert.True(tooLong);
        Assert.False(EndingNote.TryCreate(new string('a', 499) + "\U0001F600", out _, out _));
    }

    [Fact]
    public void ToString_never_returns_the_text()
    {
        Assert.True(EndingNote.TryCreate("Three weeks without extra sweets.", out var note, out _));

        Assert.DoesNotContain("sweets", note!.Value.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("sweets", note.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("sweets", $"{note.Value}", StringComparison.Ordinal);
        Assert.Equal(new EndingNote().ToString(), note.Value.ToString());
    }

    [Fact]
    public void Two_notes_with_the_same_text_are_equal()
    {
        Assert.True(EndingNote.TryCreate("Too busy.", out var a, out _));
        Assert.True(EndingNote.TryCreate(" Too busy. ", out var b, out _));
        Assert.True(EndingNote.TryCreate("too busy.", out var c, out _));

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }
}
