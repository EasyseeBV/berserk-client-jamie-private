using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Home.Profile;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.Home.States.Profile
{
	public class ProfileMatchLogState : State
	{
		private readonly IUIService uiService;
		private readonly ICustomisationItemRepository repository;
		private readonly IGameDatabase gameDatabase;

		private CancellationTokenSource tokenSource;

		public ProfileMatchLogState( 
			IState parentState, 
			IUIService uiService,
			ICustomisationItemRepository repository,
			IGameDatabase gameDatabase) 
			: base(parentState)
		{
			this.uiService = uiService;
			this.repository = repository;
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
			uiService.Begin<ProfileMatchLogWindow>().Hide();
		}

		private async UniTask InitProfileMatchLogView(CancellationToken token)
		{
			var responce = await PlayerAPI.GetStatisticsSessions().AddLoadingTask();
			if (!responce.IsSuccess || token.IsCancellationRequested)
				return;
			var borderArtUrl = repository.GetFirstEquipped(CustomisationType.AvatarFrame).AssetData.URL;
			uiService.Begin<ProfileMatchLogWindow>()
				.WithInit(window =>
				{
					window.Clear();
					int i = 0;
					foreach (var row in responce.Data)
					{
						if (row.MatchMode == MatchMode.Tutorial)
							continue;
						
						i++;
						var matchLogRow = window.CreateRow();
						matchLogRow.Init(
							i.ToString(),
							row.SessionId.Ellipsis(10),
							row.MatchMode.ToString(),
							row.ELO.ToString(),
							GetHeroArtUrl(row.PlayedVulcaniteId),
							borderArtUrl,
							gameDatabase.GetCards(row.PlayedDeck).ToArray(),
							new DateTime((row.EndDateTime - row.GameReadyDateTime).Value.Ticks)
								.ToString("HH'h' : mm'm' : ss's'"),
							row.HasWon ? "Winner" : "Loser",
							row.HasWon ? Color.green : Color.red);
					}
				}).Show();;
		}
		
		private string GetHeroArtUrl(string id)
		{
			return gameDatabase.GetHero(id)?.ArtUrl;
		}

	}
}