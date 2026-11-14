using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.UnitTests.Application.Habits;

/// <summary>Inputs and acting users for the catalog administration tests.</summary>
public static class CatalogInputs
{
    public static ActingUser Administrator { get; } = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    public static ActingUser RegisteredUser { get; } = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

    public static CatalogHabitInput Input(
        string? name = "Reading",
        Category? category = Category.Mind,
        Intent? intent = Intent.Build,
        string? icon = "book-open",
        string? description = null,
        params (SuggestedStartingGoalFields Fields, bool IsMarked)[] goals) => new()
    {
        Name = name,
        Description = description,
        Category = category,
        Intent = intent,
        Icon = icon,
        SuggestedStartingGoals = goals.Select(g => Goal(g.Fields, g.IsMarked)).ToList(),
    };

    public static SuggestedStartingGoalInput Goal(SuggestedStartingGoalFields fields, bool isMarked = false) => new()
    {
        BuildOrReduce = fields.BuildOrReduce,
        Measurement = fields.Measurement,
        Amount = fields.Amount,
        CountUnit = fields.CountUnit,
        DurationUnit = fields.DurationUnit,
        ScheduleKind = fields.ScheduleKind,
        Days = fields.Days ?? new HashSet<DayOfWeek>(),
        Times = fields.Times,
        Period = fields.Period,
        IsMarked = isMarked,
    };
}
