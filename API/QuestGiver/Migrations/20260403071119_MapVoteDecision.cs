using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestGiver.Migrations
{
    /// <inheritdoc />
    public partial class MapVoteDecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Decision",
                table: "Votes",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Decision",
                table: "Votes");
        }
    }
}
