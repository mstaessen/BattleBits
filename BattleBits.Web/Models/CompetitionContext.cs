using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BattleBits.Web.Models
{

    public class CompetitionContext : IdentityDbContext<ApplicationUser>
    {
        public CompetitionContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
            // For now the model changes too often
            Database.SetInitializer(new CompetitionContextInitializer());
        }

        public DbSet<Competition> Competitions { get; set; }

        public DbSet<GameEntry> GameEntries { get; set; }

        public DbSet<BattleBitsCompetition> BattleBitCompetitions { get; set; }
        

        public static CompetitionContext Create()
        {
            return new CompetitionContext();
        }
    }
}