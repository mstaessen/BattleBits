namespace BattleBits.Web.Hubs
{
    public class BattleBitsPlayer
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Company { get; set; }

        public double? HighScore { get; set; }
    }
}