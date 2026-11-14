using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HT.Persistence.Migrations
{
    /// <inheritdoc />
    internal partial class HabitCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogHabits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false, collation: "Latin1_General_100_BIN2"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Intent = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Icon = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    State = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogHabits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomHabits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false, collation: "Latin1_General_100_BIN2"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Intent = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Icon = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomHabits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomHabits_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogHabitSuggestedStartingGoals",
                columns: table => new
                {
                    Position = table.Column<int>(type: "int", nullable: false),
                    CatalogHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsMarked = table.Column<bool>(type: "bit", nullable: false),
                    BuildOrReduce = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Measurement = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: true),
                    CountUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DurationUnit = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    ScheduleKind = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Days = table.Column<int>(type: "int", nullable: true),
                    Times = table.Column<int>(type: "int", nullable: true),
                    Period = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogHabitSuggestedStartingGoals", x => new { x.CatalogHabitId, x.Position });
                    table.ForeignKey(
                        name: "FK_CatalogHabitSuggestedStartingGoals_CatalogHabits_CatalogHabitId",
                        column: x => x.CatalogHabitId,
                        principalTable: "CatalogHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogHabits_NameKey",
                table: "CatalogHabits",
                column: "NameKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomHabits_OwnerId_NameKey",
                table: "CustomHabits",
                columns: new[] { "OwnerId", "NameKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogHabitSuggestedStartingGoals");

            migrationBuilder.DropTable(
                name: "CustomHabits");

            migrationBuilder.DropTable(
                name: "CatalogHabits");
        }
    }
}
