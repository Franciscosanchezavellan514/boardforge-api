using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.BoardForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class LabelsAddIndexForTeamAndNormalizedNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Labels_TeamId",
                table: "Labels");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_TeamId_NormalizedName",
                table: "Labels",
                columns: new[] { "TeamId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Labels_TeamId_NormalizedName",
                table: "Labels");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_TeamId",
                table: "Labels",
                column: "TeamId");
        }
    }
}
