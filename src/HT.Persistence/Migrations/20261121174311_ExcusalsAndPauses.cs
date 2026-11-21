using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HT.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExcusalsAndPauses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoalExcusals",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalExcusals", x => new { x.GoalId, x.Date });
                    table.ForeignKey(
                        name: "FK_GoalExcusals_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalPauses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstDay = table.Column<DateOnly>(type: "date", nullable: false),
                    LastDay = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalPauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalPauses_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalPauses_GoalId_FirstDay",
                table: "GoalPauses",
                columns: new[] { "GoalId", "FirstDay" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalExcusals");

            migrationBuilder.DropTable(
                name: "GoalPauses");
        }
    }
}
