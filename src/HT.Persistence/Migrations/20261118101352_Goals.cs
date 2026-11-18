using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HT.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Goals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoalsRevision",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreationSequence = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Goals_CatalogHabits_CatalogHabitId",
                        column: x => x.CatalogHabitId,
                        principalTable: "CatalogHabits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Goals_CustomHabits_CustomHabitId",
                        column: x => x.CustomHabitId,
                        principalTable: "CustomHabits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GoalAttempts",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalAttempts", x => new { x.GoalId, x.Number });
                    table.ForeignKey(
                        name: "FK_GoalAttempts_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalSettingsVersions",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppliesFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    BuildOrReduce = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Measurement = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: true),
                    CountUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DurationUnit = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    ScheduleKind = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Days = table.Column<int>(type: "int", nullable: true),
                    Times = table.Column<int>(type: "int", nullable: true),
                    Period = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalSettingsVersions", x => new { x.GoalId, x.AppliesFrom });
                    table.ForeignKey(
                        name: "FK_GoalSettingsVersions_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_CatalogHabitId",
                table: "Goals",
                column: "CatalogHabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_CreationSequence",
                table: "Goals",
                column: "CreationSequence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_CustomHabitId",
                table: "Goals",
                column: "CustomHabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_OwnerId",
                table: "Goals",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalAttempts");

            migrationBuilder.DropTable(
                name: "GoalSettingsVersions");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GoalsRevision",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "AspNetUsers");
        }
    }
}
