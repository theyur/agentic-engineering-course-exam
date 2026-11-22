using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HT.Persistence.Migrations
{
    /// <summary>
    /// Slice 8 (data-model.md "Tables" and "The migration's order", research R-05, ADR 0027). The order is fixed by hand
    /// for a populated database: (1) <c>AttemptNumber</c> with the default 1, so every existing version belongs to attempt
    /// 1, and the default dropped again, as the model has none; (2) the old key dropped, then the new key and the foreign
    /// key to <c>GoalAttempts</c> with no action on delete; (3) the three nullable ending columns. <c>Down</c> reverses
    /// it and is only valid while every version belongs to attempt 1.
    /// </summary>
    public partial class GoalChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // (1) Every version of slices 4 to 7 belongs to attempt 1; the model has no default value.
            migrationBuilder.AddColumn<int>(
                name: "AttemptNumber",
                table: "GoalSettingsVersions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "AttemptNumber",
                table: "GoalSettingsVersions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            // (2) The key (goal, attempt number, applies-from) and the foreign key to the attempt.
            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalSettingsVersions",
                table: "GoalSettingsVersions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalSettingsVersions",
                table: "GoalSettingsVersions",
                columns: new[] { "GoalId", "AttemptNumber", "AppliesFrom" });

            migrationBuilder.AddForeignKey(
                name: "FK_GoalSettingsVersions_GoalAttempts_GoalId_AttemptNumber",
                table: "GoalSettingsVersions",
                columns: new[] { "GoalId", "AttemptNumber" },
                principalTable: "GoalAttempts",
                principalColumns: new[] { "GoalId", "Number" });

            // (3) The ending of an attempt, null while it is not ended.
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndingDate",
                table: "GoalAttempts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndingNote",
                table: "GoalAttempts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "GoalAttempts",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // (3) reversed.
            migrationBuilder.DropColumn(
                name: "EndingDate",
                table: "GoalAttempts");

            migrationBuilder.DropColumn(
                name: "EndingNote",
                table: "GoalAttempts");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "GoalAttempts");

            // (2) reversed; valid only while every version belongs to attempt 1.
            migrationBuilder.DropForeignKey(
                name: "FK_GoalSettingsVersions_GoalAttempts_GoalId_AttemptNumber",
                table: "GoalSettingsVersions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalSettingsVersions",
                table: "GoalSettingsVersions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalSettingsVersions",
                table: "GoalSettingsVersions",
                columns: new[] { "GoalId", "AppliesFrom" });

            // (1) reversed.
            migrationBuilder.DropColumn(
                name: "AttemptNumber",
                table: "GoalSettingsVersions");
        }
    }
}
