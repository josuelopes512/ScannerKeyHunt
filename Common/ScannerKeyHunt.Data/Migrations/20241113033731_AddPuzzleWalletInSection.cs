using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScannerKeyHunt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPuzzleWalletInSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_PuzzleWallets_PuzzleWalletId",
                table: "Sections");

            migrationBuilder.AlterColumn<Guid>(
                name: "PuzzleWalletId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_PuzzleWallets_PuzzleWalletId",
                table: "Sections",
                column: "PuzzleWalletId",
                principalTable: "PuzzleWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_PuzzleWallets_PuzzleWalletId",
                table: "Sections");

            migrationBuilder.AlterColumn<Guid>(
                name: "PuzzleWalletId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_PuzzleWallets_PuzzleWalletId",
                table: "Sections",
                column: "PuzzleWalletId",
                principalTable: "PuzzleWallets",
                principalColumn: "Id");
        }
    }
}
