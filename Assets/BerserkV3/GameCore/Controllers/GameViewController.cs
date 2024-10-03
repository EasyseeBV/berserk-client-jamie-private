using Audio;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models.API;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using Vulcan.Audio;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class GameViewController : DisposableWithCts, IInitializable
	{
		private readonly IGameView gameView;
		private readonly IGameContext gameContext;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameRepository gameRepository;

		public GameViewController(
			IGameView gameView,
			IGameContext gameContext,
			IGameLogicEventsSource gameLogicEventsSource,
			IGameRepository gameRepository)
		{
			this.gameView = gameView;
			this.gameContext = gameContext;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameRepository = gameRepository;
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<TurnGame>(OnTurnChanged, Token);
			gameView.SetActiveteYourTurn(false);
		}

		private void OnTurnChanged(TurnGame turnGame)
		{
			if (turnGame.RuntimeData.Turn == 1)
				ShowOpponentCommend();
			
			if (turnGame.RuntimeData.OwnerId != gameRepository.SelfId)
				return;
			
			gameView.SetActiveteYourTurn(true);
			gameView.AnimateTurn();
			AudioController.Play(Clip.YourTurn);
		}

		private async void ShowOpponentCommend()
		{
			var commendModel = new CommendModel
			{
				UserId = gameRepository.OpponentId
			};
			var response = await GameAPI.PostGetUserCommends(commendModel);
			if (!response)
				return;
			
			gameView.SetActiveCommendText(true);
			gameView.SetCommendText($"COMMENDATION LEVEL {response.Data.Value}");
			gameView.AnimateCommend();
		}
	}
}