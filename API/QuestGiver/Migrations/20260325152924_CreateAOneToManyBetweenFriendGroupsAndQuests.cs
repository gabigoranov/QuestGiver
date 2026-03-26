using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestGiver.Migrations
{
    /// <inheritdoc />
    public partial class CreateAOneToManyBetweenFriendGroupsAndQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendGroups_Quests_CurrentQuestId",
                table: "FriendGroups");

            migrationBuilder.DropIndex(
                name: "IX_FriendGroups_CurrentQuestId",
                table: "FriendGroups");

            migrationBuilder.DropColumn(
                name: "CurrentQuestId",
                table: "FriendGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "FriendGroupId",
                table: "Quests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Quests_FriendGroupId",
                table: "Quests",
                column: "FriendGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_FriendGroups_FriendGroupId",
                table: "Quests",
                column: "FriendGroupId",
                principalTable: "FriendGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quests_FriendGroups_FriendGroupId",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quests_FriendGroupId",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "FriendGroupId",
                table: "Quests");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentQuestId",
                table: "FriendGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendGroups_CurrentQuestId",
                table: "FriendGroups",
                column: "CurrentQuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendGroups_Quests_CurrentQuestId",
                table: "FriendGroups",
                column: "CurrentQuestId",
                principalTable: "Quests",
                principalColumn: "Id");
        }
    }
}
