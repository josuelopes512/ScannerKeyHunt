using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScannerKeyHunt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPuzzleWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PuzzleWalletId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PuzzleWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PuzzleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Disabled = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleWallets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_PuzzleWalletId",
                table: "Sections",
                column: "PuzzleWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_PuzzleWallets_PuzzleWalletId",
                table: "Sections",
                column: "PuzzleWalletId",
                principalTable: "PuzzleWallets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_PuzzleWallets_PuzzleWalletId",
                table: "Sections");

            migrationBuilder.DropTable(
                name: "PuzzleWallets");

            migrationBuilder.DropIndex(
                name: "IX_Sections_PuzzleWalletId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "PuzzleWalletId",
                table: "Sections");
        }
    }
}
