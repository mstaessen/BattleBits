using System.Data.Entity;

namespace BattleBits.Web.Models
{
    public class CompetitionContextInitializer : DropCreateDatabaseIfModelChanges<CompetitionContext>
    {
        protected override void Seed(CompetitionContext context)
        {
            context.BattleBitCompetitions.Add(new BattleBitsCompetition("BattleBits @ 10 Years VISUG"));
            base.Seed(context);
        }
    }
}