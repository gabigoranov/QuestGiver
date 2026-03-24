using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestGiver.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "Tokens",
                newName: "ExpirationDateTime");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Tokens",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Tokens");

            migrationBuilder.RenameColumn(
                name: "ExpirationDateTime",
                table: "Tokens",
                newName: "ExpirationDate");
        }
    }
}
