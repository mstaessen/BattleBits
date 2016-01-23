using System.Data.Entity;

namespace BattleBits.Web.Models
{
    /// <summary>
    /// TODO Fake EF Context for testing purposes, make real EF class later
    /// </summary>
    public class CompetitionContext : DbContext
    {
        public CompetitionContext()
        {
            // For now the model changes to often
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CompetitionContext>());
        }

        public DbSet<Competition> Competitions { get; set; }
        public DbSet<GameEntry> GameEntries { get; set; }
        //public DbSet<ApplicationUser> Users { get; set; } // TODO
    }
}