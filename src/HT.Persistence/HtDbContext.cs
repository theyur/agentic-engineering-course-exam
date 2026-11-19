using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Persistence.Goals;
using HT.Persistence.Habits;
using HT.Persistence.Identity;
using HT.Persistence.Records;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HT.Persistence;

/// <summary>
/// Identity's user tables (no role tables, research R-02), the habit tables of slice 2, the goal tables of slice 4 and
/// the record and note tables of slice 5.
/// </summary>
internal sealed class HtDbContext(DbContextOptions<HtDbContext> options)
    : IdentityUserContext<HtUser, Guid>(options)
{
    /// <summary>A binary collation, so a unique index compares exactly what the domain's equality compares (research R-04, R-03).</summary>
    public const string BinaryCollation = "Latin1_General_100_BIN2";

    /// <summary>The length of a named time zone column (data-model.md "Tables").</summary>
    public const int TimeZoneMaxLength = 64;

    public DbSet<CatalogHabitRow> CatalogHabits => Set<CatalogHabitRow>();

    public DbSet<CustomHabitRow> CustomHabits => Set<CustomHabitRow>();

    public DbSet<GoalRow> Goals => Set<GoalRow>();

    public DbSet<GoalSettingsVersionRow> GoalSettingsVersions => Set<GoalSettingsVersionRow>();

    public DbSet<GoalAttemptRow> GoalAttempts => Set<GoalAttemptRow>();

    public DbSet<GoalRecordRow> GoalRecords => Set<GoalRecordRow>();

    public DbSet<GoalNoteRow> GoalNotes => Set<GoalNoteRow>();

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

            // Slice 4 (research R-04, R-08): written at insert, then only by the stores' conditional statements, so a
            // stale user saved by Identity's full-row update never rolls back the zone, the name or the revision.
            user.Property(u => u.TimeZone).HasMaxLength(TimeZoneMaxLength);
            user.Property(u => u.Name).HasMaxLength(AccountName.MaxLength);
            user.Property(u => u.GoalsRevision).HasDefaultValue(0);
            user.Property(u => u.TimeZone).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            user.Property(u => u.Name).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            user.Property(u => u.CreatedAt).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            user.Property(u => u.GoalsRevision).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
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

        // Slice 4 (data-model.md "Tables"): no check constraints and no default values but the identity column.
        builder.Entity<GoalRow>(goal =>
        {
            goal.ToTable("Goals");
            goal.HasKey(g => g.Id);
            goal.Property(g => g.Id).ValueGeneratedNever();
            goal.HasOne<HtUser>().WithMany().HasForeignKey(g => g.OwnerId).OnDelete(DeleteBehavior.Cascade);
            goal.HasIndex(g => g.OwnerId).HasDatabaseName("IX_Goals_OwnerId");
            goal.HasOne(g => g.CatalogHabit).WithMany().HasForeignKey(g => g.CatalogHabitId).OnDelete(DeleteBehavior.NoAction);
            goal.HasIndex(g => g.CatalogHabitId).HasDatabaseName("IX_Goals_CatalogHabitId");
            goal.HasOne(g => g.CustomHabit).WithMany().HasForeignKey(g => g.CustomHabitId).OnDelete(DeleteBehavior.NoAction);
            goal.HasIndex(g => g.CustomHabitId).HasDatabaseName("IX_Goals_CustomHabitId");
            goal.Property(g => g.Name).HasMaxLength(GoalName.MaxLength);
            goal.Property(g => g.CreationSequence).UseIdentityColumn();
            goal.HasIndex(g => g.CreationSequence).IsUnique().HasDatabaseName("IX_Goals_CreationSequence");
            goal.HasMany(g => g.Versions).WithOne().HasForeignKey(v => v.GoalId).OnDelete(DeleteBehavior.Cascade);
            goal.HasMany(g => g.Attempts).WithOne().HasForeignKey(a => a.GoalId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GoalSettingsVersionRow>(version =>
        {
            version.ToTable("GoalSettingsVersions");
            version.HasKey(v => new { v.GoalId, v.AppliesFrom });
            version.Property(v => v.BuildOrReduce).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            version.Property(v => v.Measurement).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            version.Property(v => v.CountUnit).HasMaxLength(SuggestedStartingGoal.CountUnitMaxLength);
            version.Property(v => v.DurationUnit).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
            version.Property(v => v.ScheduleKind).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
            version.Property(v => v.Period).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
            version.Property(v => v.TimeZone).HasMaxLength(TimeZoneMaxLength).IsRequired();
        });

        builder.Entity<GoalAttemptRow>(attempt =>
        {
            attempt.ToTable("GoalAttempts");
            attempt.HasKey(a => new { a.GoalId, a.Number });
            attempt.Property(a => a.Number).ValueGeneratedNever();
        });

        // Slice 5 (data-model.md "Tables"): no check constraints; the mapping keeps the value rule (research R-06).
        builder.Entity<GoalRecordRow>(record =>
        {
            record.ToTable("GoalRecords");
            record.HasKey(r => r.Id).IsClustered(false);
            record.Property(r => r.Id).ValueGeneratedNever();
            record.HasOne<GoalRow>().WithMany().HasForeignKey(r => r.GoalId).OnDelete(DeleteBehavior.Cascade);
            record.Property(r => r.Date).HasColumnType("date");
            record.Property(r => r.Answer).HasColumnType("tinyint");
            record.Property(r => r.Sequence).UseIdentityColumn();
            record.HasIndex(r => new { r.GoalId, r.Date, r.Sequence })
                .IsClustered()
                .HasDatabaseName("IX_GoalRecords_GoalId_Date_Sequence");
        });

        builder.Entity<GoalNoteRow>(note =>
        {
            note.ToTable("GoalNotes");
            note.HasKey(n => new { n.GoalId, n.EntryDate });
            note.HasOne<GoalRow>().WithMany().HasForeignKey(n => n.GoalId).OnDelete(DeleteBehavior.Cascade);
            note.Property(n => n.EntryDate).HasColumnType("date");
            note.Property(n => n.Text).HasMaxLength(RecordLimits.MaxNoteLength).IsRequired();
        });
    }
}
