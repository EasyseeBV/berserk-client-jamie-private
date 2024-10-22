using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.General.Profile.Collection;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Lobby.Home.States.Profile
{
	public class ProfileCollectionState : State
	{
		private readonly IUIService uiService;
		private readonly IGameDatabase gameDatabase;

		private CancellationTokenSource tokenSource;

		public ProfileCollectionState(
			IState parentState, 
			IUIService uiService,
			IGameDatabase gameDatabase)
			: base(parentState)
		{
			this.uiService = uiService;
			this.gameDatabase = gameDatabase;
		}
		
		public override void OnEnter(params object[] args)
		{
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = new();
			var token = tokenSource.Token;
			InitProfileMatchLogView(token).Forget();
		}

		public override void OnExit()
		{
			base.OnExit();
			tokenSource?.Cancel();
			uiService.Begin<ProfileCollectionWindow>().Hide();
		}

		private async UniTask InitProfileMatchLogView(CancellationToken token)
		{
			var responce = await PlayerAPI.GetCardStatistics().AddLoadingTask();
			if (!responce.IsSuccess || token.IsCancellationRequested)
				return;
			uiService.Begin<ProfileCollectionWindow>()
				.WithInit(window =>
				{
					window.Clear();
					int i = 0;
					foreach (var row in responce.Data)
					{
						i++;
						var profileCollectionRow = window.CreateRow();
						var cardData = gameDatabase.GetCard(row.CardId);
						profileCollectionRow.Init(
							i.ToString(),
							row.ELO.ToString(),
							row.Winrate + "%",
							row.CountGames.ToString(),
							cardData,
							cardData.Rarity.ToString(),
							cardData.Title);
					}
				}).Show();;
		}
	}
}