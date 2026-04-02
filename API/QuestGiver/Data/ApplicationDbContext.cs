using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using System.Diagnostics;

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
        public DbSet<Models.Vote> Votes { get; set; }
        public DbSet<Models.UserVote> UserVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Token>()
            .HasIndex(t => t.RefreshToken)
            .IsUnique();

            // Configure TPH
            modelBuilder.Entity<Vote>().ToTable(nameof(Votes));
            modelBuilder.Entity<CompletionVote>();
            modelBuilder.Entity<SkipVote>();

            modelBuilder.Entity<Vote>()
                .HasDiscriminator<VoteType>(x => x.Discriminator)
                .HasValue<CompletionVote>(VoteType.CompletionVote)
                .HasValue<SkipVote>(VoteType.SkipVote);

            // Configure the composite key for the user votes table;
            modelBuilder.Entity<UserVote>()
                .HasKey(ufg => new { ufg.UserId, ufg.VoteId });

            // Configure the composite key for the UserFriendGroup join table
            modelBuilder.Entity<Models.UserFriendGroup>()
                .HasKey(ufg => new { ufg.UserId, ufg.FriendGroupId });
        }
    }
}
