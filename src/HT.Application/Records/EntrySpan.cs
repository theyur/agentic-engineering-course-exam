namespace HT.Application.Records;

/// <summary>
/// One commitment or evaluation period as the record store sees it (research R-05): the key its note is filed under
/// (the commitment's date, the day, or the week's Monday) and its first and last date. A commitment has both dates
/// equal.
/// </summary>
public sealed record EntrySpan(DateOnly Key, DateOnly FirstDate, DateOnly LastDate);
