using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F002;

/// <summary>
/// Arranges the accounts and habits the Given steps name, as the acceptance-criteria conventions define them:
/// "the administrator `a`" is administrator creation with <c>admin2026</c> and a sign-in; "the registered user
/// `x`" is registration with <c>walking42</c> and a sign-in; "a Draft / Published / Unpublished `N`" is created by
/// the item's administrator, then published, then unpublished as its state says.
/// </summary>
public sealed class F002Arrange(F002Context context, F002Driver driver)
{
    public async Task<ActingUser> AdministratorAsync(string email, string password = F002Text.AdministratorPassword)
    {
        var created = await driver.CreateAdministratorAsync(email, password);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var user = await SignInAsync(email, password);
        Assert.Equal(Role.Administrator, user.Role);
        context.CurrentAdministratorEmail = email;
        return user;
    }

    public async Task<ActingUser> RegisteredUserAsync(string email, string password = F002Text.RegisteredUserPassword)
    {
        var registered = await driver.RegisterAsync(email, password);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        var user = await SignInAsync(email, password);
        Assert.Equal(Role.RegisteredUser, user.Role);
        context.CurrentRegisteredUserEmail = email;
        return user;
    }

    public async Task<ActingUser> SignInAsync(string email, string password)
    {
        var signIn = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        var user = signIn.ActingUser ?? throw new InvalidOperationException($"Signing in `{email}` gave no acting user.");
        context.Accounts[email] = user;
        context.Passwords[email] = password;
        return user;
    }

    /// <summary>Signs in with the password this scenario knows for the account, or the last demo password.</summary>
    public Task<ActingUser> SignInKnownAsync(string email)
    {
        var password = context.Passwords.TryGetValue(email, out var known)
            ? known
            : context.LastDemoPassword ?? throw new InvalidOperationException($"No password is known for `{email}`.");
        return SignInAsync(email, password);
    }

    /// <summary>The catalog habits of a step such as "a Published `N` with the category Body and a Draft `M`".</summary>
    public async Task CatalogHabitsAsync(string habitList)
    {
        foreach (var (state, name, clause) in F002Text.ParseHabitList(habitList))
        {
            var spec = new HabitSpec();
            spec.SetName(TextValue.Of(name));
            F002Text.ApplyHabitClause(spec, clause);
            await CatalogHabitAsync(state, spec);
        }
    }

    public async Task<HabitRecord> CatalogHabitAsync(CatalogHabitState state, HabitSpec spec)
    {
        var administrator = context.Administrator;
        var created = await driver.CreateCatalogHabitAsync(administrator, spec.ToCatalogInput());
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{spec.Name}` gave no identifier.");

        if (state is CatalogHabitState.Published or CatalogHabitState.Unpublished)
        {
            var published = await driver.PublishAsync(administrator, id);
            Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        }

        if (state is CatalogHabitState.Unpublished)
        {
            var unpublished = await driver.UnpublishAsync(administrator, id);
            Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublished.Outcome);
        }

        var record = new HabitRecord(HabitKind.Catalog, id, null, spec);
        context.Habits.Add(record);
        context.CurrentHabit = record;
        return record;
    }

    public async Task<HabitRecord> CustomHabitAsync(string ownerEmail, HabitSpec spec)
    {
        var created = await driver.CreateCustomHabitAsync(context.Account(ownerEmail), spec.ToCustomInput());
        Assert.Equal(CustomHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{spec.Name}` gave no identifier.");
        var record = new HabitRecord(HabitKind.Custom, id, ownerEmail, spec);
        context.Habits.Add(record);
        context.CurrentHabit = record;
        return record;
    }

    /// <summary>A custom habit with the name and the fields of a clause such as "with the category Mind".</summary>
    public Task<HabitRecord> CustomHabitAsync(string ownerEmail, string name, string? clause)
    {
        var spec = new HabitSpec();
        spec.SetName(TextValue.Of(name));
        if (!string.IsNullOrEmpty(clause))
        {
            F002Text.ApplyFields(spec, clause);
        }

        return CustomHabitAsync(ownerEmail, spec);
    }
}
