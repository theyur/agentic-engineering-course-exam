using HT.Domain.Habits;
using HT.Persistence.Habits;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HT.Persistence;

/// <summary>Identity's user tables (no role tables, research R-02) and the habit tables of slice 2.</summary>
internal sealed class HtDbContext(DbContextOptions<HtDbContext> options)
    : IdentityUserContext<HtUser, Guid>(options)
{
    /// <summary>A binary collation, so a unique index compares exactly what the domain's equality compares (research R-04, R-03).</summary>
    public const string BinaryCollation = "Latin1_General_100_BIN2";

    public DbSet<CatalogHabitRow> CatalogHabits => Set<CatalogHabitRow>();

    public DbSet<CustomHabitRow> CustomHabits => Set<CustomHabitRow>();

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

        // No check constraints and no default values: the rules live in HT.Domain (data-model.md, ADR 0011).
        builder.Entity<CatalogHabitRow>(habit =>
        {
            habit.ToTable("CatalogHabits");
            habit.HasKey(h => h.Id);
            habit.Property(h => h.Id).ValueGeneratedNever();
            habit.Property(h => h.Name).HasMaxLength(HabitName.MaxLength).IsRequired();
            habit.Property(h => h.NameKey).HasMaxLength(HabitName.MaxLength).IsRequired().UseCollation(BinaryCollation);
            habit.HasIndex(h => h.NameKey).IsUnique().HasDatabaseName("IX_CatalogHabits_NameKey");
            habit.Property(h => h.Description).HasMaxLength(HabitDefinition.DescriptionMaxLength);
            habit.Property(h => h.Category).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            habit.Property(h => h.Intent).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            habit.Property(h => h.Icon).HasMaxLength(32).IsUnicode(false).IsRequired();
            habit.Property(h => h.State).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();

            habit.OwnsMany(h => h.SuggestedStartingGoals, goal =>
            {
                goal.ToTable("CatalogHabitSuggestedStartingGoals");
                goal.WithOwner().HasForeignKey("CatalogHabitId");
                goal.HasKey("CatalogHabitId", nameof(SuggestedStartingGoalRow.Position));
                goal.Property(g => g.Position).ValueGeneratedNever();
                goal.Property(g => g.BuildOrReduce).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
                goal.Property(g => g.Measurement).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
                goal.Property(g => g.CountUnit).HasMaxLength(SuggestedStartingGoal.CountUnitMaxLength);
                goal.Property(g => g.DurationUnit).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
                goal.Property(g => g.ScheduleKind).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
                goal.Property(g => g.Period).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
            });
        });

        builder.Entity<CustomHabitRow>(habit =>
        {
            habit.ToTable("CustomHabits");
            habit.HasKey(h => h.Id);
            habit.Property(h => h.Id).ValueGeneratedNever();
            habit.HasOne<HtUser>().WithMany().HasForeignKey(h => h.OwnerId).OnDelete(DeleteBehavior.Cascade);
            habit.Property(h => h.Name).HasMaxLength(HabitName.MaxLength).IsRequired();
            habit.Property(h => h.NameKey).HasMaxLength(HabitName.MaxLength).IsRequired().UseCollation(BinaryCollation);
            habit.HasIndex(h => new { h.OwnerId, h.NameKey }).IsUnique().HasDatabaseName("IX_CustomHabits_OwnerId_NameKey");
            habit.Property(h => h.Description).HasMaxLength(HabitDefinition.DescriptionMaxLength);
            habit.Property(h => h.Category).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            habit.Property(h => h.Intent).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            habit.Property(h => h.Icon).HasMaxLength(32).IsUnicode(false).IsRequired();
        });
    }
}
