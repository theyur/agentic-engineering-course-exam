using HT.Domain.Exemptions;

namespace HT.UnitTests.Domain.Exemptions;

/// <summary>
/// FR-001, research R-02: a reason is trimmed and has 1 to 200 UTF-16 code units; empty after trimming is no reason;
/// it never prints its text (HP-009).
/// </summary>
public class ExemptionReasonTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(" \t\n ")]
    public void Null_empty_or_blank_is_no_reason(string? text)
    {
        Assert.True(ExemptionReason.TryCreate(text, out var reason, out var tooLong));

        Assert.False(tooLong);
        Assert.Null(reason);
    }

    [Fact]
    public void A_padded_reason_is_trimmed()
    {
        Assert.True(ExemptionReason.TryCreate("  Ill  ", out var reason, out var tooLong));

        Assert.False(tooLong);
        Assert.NotNull(reason);
        Assert.Equal("Ill", reason.Value);
    }

    [Fact]
    public void A_reason_of_200_characters_is_kept()
    {
        var text = new string('a', 200);

        Assert.True(ExemptionReason.TryCreate(text, out var reason, out var tooLong));

        Assert.False(tooLong);
        Assert.NotNull(reason);
        Assert.Equal(text, reason.Value);
        Assert.Equal(200, ExemptionLimits.MaxReasonLength);
    }

    [Fact]
    public void A_reason_of_201_characters_is_too_long()
    {
        Assert.False(ExemptionReason.TryCreate(new string('a', 201), out var reason, out var tooLong));

        Assert.True(tooLong);
        Assert.Null(reason);
    }

    [Fact]
    public void A_reason_of_201_characters_with_surrounding_spaces_that_trims_to_200_is_kept()
    {
        var text = new string('a', 200);

        Assert.True(ExemptionReason.TryCreate(" " + text, out var leading, out var leadingTooLong));
        Assert.True(ExemptionReason.TryCreate(" " + text + "  ", out var both, out var bothTooLong));

        Assert.False(leadingTooLong);
        Assert.False(bothTooLong);
        Assert.Equal(text, leading?.Value);
        Assert.Equal(text, both?.Value);
    }

    [Fact]
    public void A_surrogate_pair_counts_as_two_units()
    {
        // U+1F600 is one character on screen and two UTF-16 code units.
        const string smile = "\U0001F600";
        Assert.Equal(2, smile.Length);

        Assert.True(ExemptionReason.TryCreate(string.Concat(Enumerable.Repeat(smile, 100)), out var hundred, out _));
        Assert.NotNull(hundred);

        Assert.False(ExemptionReason.TryCreate(string.Concat(Enumerable.Repeat(smile, 100)) + "a", out var tooMany, out var tooLong));
        Assert.True(tooLong);
        Assert.Null(tooMany);
    }

    [Fact]
    public void Reasons_with_the_same_text_are_equal_by_ordinal_comparison()
    {
        Assert.True(ExemptionReason.TryCreate("Ill", out var first, out _));
        Assert.True(ExemptionReason.TryCreate(" Ill ", out var second, out _));
        Assert.True(ExemptionReason.TryCreate("ill", out var other, out _));

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.NotEqual(first, other);
        Assert.False(first.Equals(null));
        Assert.False(first.Equals("Ill"));
    }

    [Fact]
    public void A_reason_never_prints_its_text()
    {
        Assert.True(ExemptionReason.TryCreate("Travelling", out var reason, out _));

        Assert.NotNull(reason);
        Assert.DoesNotContain("Travelling", reason.ToString(), StringComparison.Ordinal);
    }
}
