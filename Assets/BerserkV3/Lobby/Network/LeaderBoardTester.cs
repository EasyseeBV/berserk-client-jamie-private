using UnityEngine;
using RR.Core.DebugSystem;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using BerserkV3.Lobby.LeaderBoard;

// create empty obj in scn Lobby->canvas/ add LeaderBoardTester scrpt on obj
namespace BerserkV3.Lobby.Network
{
    public class LeaderBoardTester : MonoBehaviour
    {
        private void Start()
        {
            TestLeaderBoardAsync().Forget();
        }

        private async UniTask TestLeaderBoardAsync()
        {
            await TestLeagueLeaderBoard();
            await TestAllPlayers();
            await TestFactionsLeaderBoard();
            await TestPlayersFactionsLeaderBoard();
            await TestWhoBeatsWho();
            await TestWhoBeatsWhoByPlayer();
        }

        private async UniTask TestLeagueLeaderBoard()
        {
            var data = await LeaderBoardApplicationAdapter.Application.GetLeagueLeaderBoard(
                "65da744a-852a-4d9b-879d-7ef37df7e558");
            RRLogger.Warning("=== LeagueLeaderBoard ===");
            RRLogger.Warning(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private async UniTask TestAllPlayers()
        {
            var data = await LeaderBoardApplicationAdapter.Application.GetAllPlayers();
            RRLogger.Warning("=== AllPlayers ===");
            RRLogger.Warning(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private async UniTask TestFactionsLeaderBoard()
        {
            var data = await LeaderBoardApplicationAdapter.Application.GetFactionsLeaderBoard();
            RRLogger.Warning("=== FactionsLeaderBoard ===");
            RRLogger.Warning(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private async UniTask TestPlayersFactionsLeaderBoard()
        {
            var data = await LeaderBoardApplicationAdapter.Application.GetPlayersFactionsLeaderBoard();
            RRLogger.Warning("=== PlayersFactionsLeaderBoard ===");
            RRLogger.Warning(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private async UniTask TestWhoBeatsWho()
        {
            var data = await LeaderBoardApplicationAdapter.Application.GetWhoBeatsWho();
            RRLogger.Warning("=== WhoBeatsWho ===");
            RRLogger.Warning(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        
        private async UniTask TestWhoBeatsWhoByPlayer()
        {
	        var data = await LeaderBoardApplicationAdapter.Application.GetWhoBeatsWhoByPlayer(
		        "berserkgaming597");
	        RRLogger.Warning("=== WhoBeatsWhoByPlayer ===");
	        RRLogger.Warning(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
