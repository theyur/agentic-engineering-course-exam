using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HT.Persistence;

/// <summary>Identity's user tables only; no role tables (research R-02).</summary>
internal sealed class HtDbContext(DbContextOptions<HtDbContext> options)
    : IdentityUserContext<HtUser, Guid>(options)
{
    /// <summary>A binary collation, so the unique index compares exactly what <c>Email</c> equality compares (research R-04).</summary>
    public const string BinaryCollation = "Latin1_General_100_BIN2";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<HtUser>(user =>
        {
            user.Property(u => u.NormalizedUserName).UseCollation(BinaryCollation);
            user.Property(u => u.NormalizedEmail).UseCollation(BinaryCollation);

            user.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            user.Property(u => u.FailedSignInCount).HasDefaultValue(0);

            // Only TrySaveSignInBlockAsync writes the block; UserManager.UpdateAsync never overwrites it (research R-05).
            user.Property(u => u.FailedSignInCount).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            user.Property(u => u.SignInBlockedSince).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        });
    }
}
