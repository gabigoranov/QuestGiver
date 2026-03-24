using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Models;

namespace QuestGiver.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Quest> Quests { get; set; }
        public DbSet<Models.FriendGroup> FriendGroups { get; set; }
        public DbSet<Models.UserFriendGroup> UserFriendGroups { get; set; }
        public DbSet<Models.Token> Tokens { get; set; }
        public DbSet<Models.UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Token>()
            .HasIndex(t => t.RefreshToken)
            .IsUnique();

            // Configure the composite key for the UserFriendGroup join table
            modelBuilder.Entity<Models.UserFriendGroup>()
                .HasKey(ufg => new { ufg.UserId, ufg.FriendGroupId });
        }
    }
}
