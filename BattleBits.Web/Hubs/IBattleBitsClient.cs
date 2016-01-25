using BattleBits.Web.Events;

namespace BattleBits.Web.Hubs
{
    public interface IBattleBitsClient
    {
        void GameScheduled(BattleBitsGameScheduledEvent evt);

        void GameCancelled();

        void PlayerJoined(BattleBitsPlayerJoinedEvent evt);

        void PlayerLeft(BattleBitsPlayerLeftEvent evt);

        void GameStarted(BattleBitsGameStartedEvent evt);

        void GameEnded(BattleBitsGameEndedEvent evt);
    }
}