using BattleBits.Web.Games.BattleBits.Events;

namespace BattleBits.Web.Games.BattleBits
{
    public interface IBattleBitsClient
    {
        void GameScheduled(BattleBitsGameScheduledEvent evt);

        void GameCancelled();

        void PlayerJoined(BattleBitsPlayerJoinedEvent evt);

        void PlayerLeft(BattleBitsPlayerLeftEvent evt);

        void GameStarted(BattleBitsGameStartedEvent evt);

        void GameEnded(BattleBitsGameEndedEvent evt);

        void PlayerScored(BattleBitsPlayerScoredEvent evt);
    }
}