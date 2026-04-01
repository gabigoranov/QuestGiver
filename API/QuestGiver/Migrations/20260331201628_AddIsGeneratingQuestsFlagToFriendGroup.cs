using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestGiver.Migrations
{
    /// <inheritdoc />
    public partial class AddIsGeneratingQuestsFlagToFriendGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGeneratingQuests",
                table: "FriendGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGeneratingQuests",
                table: "FriendGroups");
        }
    }
}
