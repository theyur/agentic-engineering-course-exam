using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Persistence.Habits;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HT.Persistence;

/// <summary>The composition entry point of HT.Persistence (ADR 0003, ADR 0004).</summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database context on SQL Server, Identity's user services with options no stricter than the
    /// domain rules (research R-03), the account store and the habit stores. Adds no token providers, roles or
    /// sign-in manager (research R-02). EF Core never logs a database error text: such a text can carry a custom
    /// habit's name (HP-015, research R-14); the stores' own exceptions carry no field value. Sensitive-data
    /// logging stays off.
    /// </summary>
    public static IServiceCollection AddHtPersistence(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<HtDbContext>(options => options
            .UseSqlServer(connectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.SaveChangesFailed, RelationalEventId.CommandError)));

        services.AddIdentityCore<HtUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                // Identity checks characters only when the set is not empty.
                options.User.AllowedUserNameCharacters = string.Empty;
                options.User.RequireUniqueEmail = false;

                options.Lockout.AllowedForNewUsers = false;

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<HtDbContext>()
            .AddClaimsPrincipalFactory<HtUserClaimsPrincipalFactory>();

        services.Replace(ServiceDescriptor.Scoped<ILookupNormalizer, EmailLookupNormalizer>());
        services.AddScoped<IAccountStore, AccountStore>();
        services.AddScoped<ICatalogHabitStore, CatalogHabitStore>();
        return services;
    }
}
