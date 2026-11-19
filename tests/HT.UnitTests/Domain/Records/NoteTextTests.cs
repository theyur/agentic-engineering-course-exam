using HT.Domain.Records;

namespace HT.UnitTests.Domain.Records;

/// <summary>FR-007, research R-02: a note is trimmed and has 1 to 500 characters; empty after trimming is no note.</summary>
public class NoteTextTests
{
    [Fact]
    public void A_note_of_500_characters_after_trimming_is_kept()
    {
        var text = new string('a', 500);

        Assert.True(NoteText.TryCreate("  " + text + "  ", out var note, out var tooLong));

        Assert.False(tooLong);
        Assert.NotNull(note);
        Assert.Equal(text, note.Value);
        Assert.Equal(500, RecordLimits.MaxNoteLength);
    }

    [Fact]
    public void A_note_of_501_characters_after_trimming_is_too_long()
    {
        Assert.False(NoteText.TryCreate("  " + new string('a', 501) + "  ", out var note, out var tooLong));

        Assert.True(tooLong);
        Assert.Null(note);
    }

    [Fact]
    public void A_padded_note_is_trimmed()
    {
        Assert.True(NoteText.TryCreate("  Back was tired.  ", out var note, out var tooLong));

        Assert.False(tooLong);
        Assert.NotNull(note);
        Assert.Equal("Back was tired.", note.Value);
    }

    [Theory]
    [InlineData("     ")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" \t\n ")]
    public void Only_spaces_empty_or_null_is_no_note(string? text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out var tooLong));

        Assert.False(tooLong);
        Assert.Null(note);
    }

    [Fact]
    public void Notes_with_the_same_text_are_equal()
    {
        Assert.True(NoteText.TryCreate("Fell asleep early.", out var first, out _));
        Assert.True(NoteText.TryCreate(" Fell asleep early. ", out var second, out _));
        Assert.True(NoteText.TryCreate("fell asleep early.", out var other, out _));

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.NotEqual(first, other);
    }

    [Fact]
    public void A_note_never_prints_its_text()
    {
        Assert.True(NoteText.TryCreate("Back was tired.", out var note, out _));

        Assert.NotNull(note);
        Assert.DoesNotContain("tired", note.ToString(), StringComparison.Ordinal);
    }
}
