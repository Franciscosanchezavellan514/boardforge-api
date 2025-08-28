using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.BoardForge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIdToRelatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardComments_Cards_CardId",
                table: "CardComments");

            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Cards_CardId",
                table: "CardLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Labels_LabelId",
                table: "CardLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_Labels_Teams_TeamId",
                table: "Labels");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMemberships_Teams_TeamId",
                table: "TeamMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMemberships_Users_UserId",
                table: "TeamMemberships");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BoardColumns");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "BoardColumns");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "BoardColumns",
                newName: "CreateAt");

            migrationBuilder.AlterColumn<int>(
                name: "BoardColumnId",
                table: "Cards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Cards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "CardAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "BoardColumns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_TeamId",
                table: "Cards",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CardAttachments_TeamId",
                table: "CardAttachments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_TeamId",
                table: "BoardColumns",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardColumns_Teams_TeamId",
                table: "BoardColumns",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardAttachments_Teams_TeamId",
                table: "CardAttachments",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardComments_Cards_CardId",
                table: "CardComments",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Cards_CardId",
                table: "CardLabels",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Labels_LabelId",
                table: "CardLabels",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Teams_TeamId",
                table: "Cards",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Labels_Teams_TeamId",
                table: "Labels",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMemberships_Teams_TeamId",
                table: "TeamMemberships",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMemberships_Users_UserId",
                table: "TeamMemberships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardColumns_Teams_TeamId",
                table: "BoardColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_CardAttachments_Teams_TeamId",
                table: "CardAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_CardComments_Cards_CardId",
                table: "CardComments");

            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Cards_CardId",
                table: "CardLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Labels_LabelId",
                table: "CardLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Teams_TeamId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Labels_Teams_TeamId",
                table: "Labels");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMemberships_Teams_TeamId",
                table: "TeamMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMemberships_Users_UserId",
                table: "TeamMemberships");

            migrationBuilder.DropIndex(
                name: "IX_Cards_TeamId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_CardAttachments_TeamId",
                table: "CardAttachments");

            migrationBuilder.DropIndex(
                name: "IX_BoardColumns_TeamId",
                table: "BoardColumns");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "CardAttachments");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "BoardColumns");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "BoardColumns",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "BoardColumnId",
                table: "Cards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CardLabels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "CardLabels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CardLabels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "CardLabels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CardLabels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "CardLabels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BoardColumns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "BoardColumns",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CardComments_Cards_CardId",
                table: "CardComments",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Cards_CardId",
                table: "CardLabels",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Labels_LabelId",
                table: "CardLabels",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Labels_Teams_TeamId",
                table: "Labels",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMemberships_Teams_TeamId",
                table: "TeamMemberships",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMemberships_Users_UserId",
                table: "TeamMemberships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
