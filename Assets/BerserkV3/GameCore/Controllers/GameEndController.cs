using System.Collections.Generic;
using Audio;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models.API;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Customisations;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network;
using Lobby;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.MatchMaking.Leagues.Data;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Vulcan.Audio;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class GameEndController : DisposableWithCts, IInitializable
	{
		private readonly IGameCustomisationApplication customisationApplication;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameRepository gameRepository;
		private readonly IGameContext gameContext;
		private readonly IGameEndView gameEndView;
		private readonly ISceneService sceneService;
		private readonly List<ChangedPlayerStatistic> playerStatistics;
		private string victoryId;
		
		public GameEndController(
			IGameCustomisationApplication customisationApplication,
			IGameLogicEventsSource gameLogicEventsSource,
			IGameRepository gameRepository,
			IGameContext gameContext,
			IGameEndView gameEndView,
			ISceneService sceneService)
		{
			this.customisationApplication = customisationApplication;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameRepository = gameRepository;
			this.gameContext = gameContext;
			this.gameEndView = gameEndView;
			this.sceneService = sceneService;
			playerStatistics = new List<ChangedPlayerStatistic>();
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<EndGame>(HandleEndGame, Token);
			gameLogicEventsSource.Subscribe<Commend>(_ => PlayCommend(gameRepository.SelfId == victoryId), Token);
			gameLogicEventsSource.Subscribe<ChangedPlayerStatistic>(playerStatistics.Add, Token);
			gameEndView.OnCommend += SendCommend;
			gameEndView.OnMenu += HandleGameLeft; 
			gameEndView.OnPlayAgain += HandlePlayAgain;
		}

		public override void Dispose()
		{
			base.Dispose();
			gameEndView.OnCommend -= SendCommend;
			gameEndView.OnMenu -= HandleGameLeft;
			gameEndView.OnPlayAgain -= HandlePlayAgain;
			playerStatistics.Clear();
		}

		private void HandlePlayAgain()
		{
			if (!string.IsNullOrWhiteSpace(gameContext.RuntimeData.LeagueId)
			    && gameContext.PlayerRepository.TryGet(User.Id, out var runtimePlayer))
				User.AddRedirection(new LeagueAutoMatchRedirectArg(gameContext.RuntimeData.LeagueId, runtimePlayer.RuntimeData.DeckId));
			
			HandleGameLeft();
		}
		
		private async void HandleGameLeft()
		{
			await sceneService.LoadAsync(Scene.Lobby).AddLoadingTask();
		}

		private async void HandleEndGame(EndGame gameEndModel)
		{
		#if !PRODUCTION
			RRLogger.Log("______END_GAME______".Green() + " : " + gameEndModel);
		#endif
			
			if (string.IsNullOrEmpty(gameEndModel.WinnerId))
			{
				// used OnError directly because handling signal from server
				ErrorDispatcher.OnError.Publish(500, gameContext.GameDatabase.GetLocalization("ErrorNoWinner"));
				return;
			}
			
			if (string.IsNullOrEmpty(gameEndModel.LooserId))
			{ // used OnError directly because handling signal from server
				ErrorDispatcher.OnError.Publish(500,gameContext.GameDatabase.GetLocalization("ErrorNoLoose"));
				return;
			}

			victoryId = gameEndModel.WinnerId;
			var defeat = gameContext.PlayerRepository.Get(gameEndModel.LooserId).RuntimeData;
			var defeatHero = gameContext.GameRuntimePool.GetHeroByUserId(defeat.UserId);
			var defeatCust = customisationApplication.Get<AssetData>(defeat.UserId, CustomisationType.AvatarFrame);
			
			var victory = gameContext.PlayerRepository.Get(gameEndModel.WinnerId).RuntimeData;
			var victoryHero = gameContext.GameRuntimePool.GetHeroByUserId(victory.UserId);
			var victoryCust = customisationApplication.Get<AssetData>(victory.UserId, CustomisationType.AvatarFrame);
			var artMaskUrl = "Vulcanite_Mask";


            await UniTask.WhenAll(			
				gameEndView.SetPlayerDefeatAsync(defeat.UserName.Ellipsis(16), defeatHero.Data.ArtUrl, artMaskUrl, defeatCust.URL, GetPlayerStatistic(defeat.UserId), Token), 
				gameEndView.SetPlayerVictoryAsync(victory.UserName.Ellipsis(16), victoryHero.Data.ArtUrl, artMaskUrl, victoryCust.URL, GetPlayerStatistic(victory.UserId), Token),
				UniTask.Delay(1000));

			if (Token.IsCancellationRequested)
				return;

			gameEndView.SetAllowCommend(!defeat.IsBot && !victory.IsBot);
			gameEndView.SetAllowPlayAgain(!string.IsNullOrWhiteSpace(gameContext.RuntimeData.LeagueId));
			gameEndView.SetReason(GetReasonText(gameEndModel.Reason, defeat.UserName));
			gameEndView.Show();
			AudioController.Play(victory.UserId == User.Id ? Clip.VictoryPopup : Clip.LostPopup);
		}

		private string GetPlayerStatistic(string userId)
		{
			var leagueId = gameContext.RuntimeData.LeagueId;
    		var league = LobbyBus.Leagues.Value?.Find(x => x.Id == leagueId);
			var leagueName = league?.LeagueName;
			return playerStatistics.TryGet(x => x.UserId == userId, out var stat)
				? $"MMR: {stat.EloTotal} {(stat.EloDelta >= 0 ? $"{"+" + stat.EloDelta}".Green() : $"{stat.EloDelta}".Red())}" 
				: null;

		}

		private static string GetReasonText(GameEndReason value, string userName)
		{
			userName = (string.IsNullOrEmpty(userName) ? "The user" : userName).Orange();
			return value switch
			{
				GameEndReason.Afk => string.Format(GameDataBaseAdapter.Instance.GetLocalization("GameEndAfk"), userName),
				GameEndReason.Disconnected => string.Format(GameDataBaseAdapter.Instance.GetLocalization("GameEndDisconnected"), userName),
				GameEndReason.Surrender => string.Format(GameDataBaseAdapter.Instance.GetLocalization("GameEndSurrendered"), userName),
				GameEndReason.Baned => string.Format(GameDataBaseAdapter.Instance.GetLocalization("GameEndSurrendered"), userName),
				GameEndReason.Maintenance =>GameDataBaseAdapter.Instance.GetLocalization("GameEndMaintenance"),
				GameEndReason.MatchFailedToStart 
					or GameEndReason.MatchFailedToRecoverFromReddis
					or GameEndReason.BothBots
					=>GameDataBaseAdapter.Instance.GetLocalization("GameEndFailed"),
				GameEndReason.AbandonedTutorial =>GameDataBaseAdapter.Instance.GetLocalization("GameEndTutorialAbandoned"),
				GameEndReason.Technical =>GameDataBaseAdapter.Instance.GetLocalization("GameEndTechReason"),
				GameEndReason.MatchDodged =>GameDataBaseAdapter.Instance.GetLocalization("GameEndDodged"),
				GameEndReason.AbandonedSession =>GameDataBaseAdapter.Instance.GetLocalization("GameEndAbandoned"),
				_ => string.Empty
			};
		}

		private async void SendCommend()
		{
			PlayCommend(gameRepository.OpponentId == victoryId);
			gameEndView.SetAllowCommend(false);
			var commendModel = new CommendModel
			{
				UserId = gameRepository.OpponentId,
			};
			await GameAPI.PostUserCommend(commendModel);
		}

		private void PlayCommend(bool victory)
		{
			gameEndView.PlayCommend(victory,gameContext.GameDatabase.GetLocalization("GameEndGG"));
		}
	}
}