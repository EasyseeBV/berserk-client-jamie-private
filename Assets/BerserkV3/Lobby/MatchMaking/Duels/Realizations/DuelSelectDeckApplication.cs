using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.UI.Duels;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UI;
using Vulcan.Data;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelSelectDeckApplication : IDuelSelectDeckApplication, IDisposable
	{
		private readonly IGameDatabase gameDatabase;
		private readonly ISharedConfig sharedConfig;
		private readonly IDeckApplication deckApplication;
		private readonly ICustomisationItemRepository customisationItemRepository;
		private static LobbyDuelSelectDeckView Window => LobbyDuelSelectDeckView.Instance;
		
		private event Action OnReturnRequested;
		
		public DuelSelectDeckApplication(
			IGameDatabase gameDatabase,
			ISharedConfig sharedConfig,
			IDeckApplication deckApplication,
			ICustomisationItemRepository customisationItemRepository)
		{
			this.gameDatabase = gameDatabase;
			this.sharedConfig = sharedConfig;
			this.deckApplication = deckApplication;
			this.customisationItemRepository = customisationItemRepository;
		}

		public async UniTask OpenAsync(Action onReturn = null) // TODO Subscribe to update when user's subscription has expired
		{
			OnReturnRequested += () => onReturn?.Invoke();
			await RefreshAsync().AddLoadingTask();
			
			if (Window.VisibleState != VisibleState.Visible)
				Window.Show();
		}

		public async UniTask CloseAsync()
		{
			try
			{
				if (deckApplication.Current == null)
					await Select(deckApplication.All.FirstOrDefault()?.Id).AddLoadingTask();
			}
			catch (Exception e)
			{
				NotifyClientException(e.Message);
			}
			
			if (Window.VisibleState == VisibleState.Visible)
				Window.Close();

			var mem = OnReturnRequested;
			OnReturnRequested = null;
			mem?.Invoke();
		}

		public async UniTask Select(string deckId)
		{
			var deckData = deckApplication.All.FirstOrDefault(x => x.Id == deckId);
			if (deckData == null)
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_SelectDeckFailed_NoDeckExist"));
				return;
			}

			try
			{
				await deckApplication.SelectAsync(deckData);
				await CloseAsync();
			}
			catch (Exception e)
			{
				NotifyClientException(e.Message);
			}
		}

		public bool Validate(string deckId)
		{
			var deckData = deckApplication.All.FirstOrDefault(x => x.Id == deckId);
			if (deckData == null)
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_ValidateDeck_NoDeckSelected"));
				return false;
			}
			
			var ownedHero = User.OwnedVulcanites.FirstOrDefault(x => x.Id == deckData.OwnedVulcaniteId);
			if (!ownedHero.IsValid())
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_ValidateDeck_NotValidVulcanite"));
				return false;
			}

			if (!deckData.IsValidCards())
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_ValidateDeck_NotValidCards"));
				return false;
			}
			
			if (deckData.OwnedCardIds.Count < sharedConfig.MinCardsInDeck)
			{
				NotifyClientException(string.Format(gameDatabase.GetLocalization("ClientDuels_ValidateDeck_NotEnoughCards"), sharedConfig.MinCardsInDeck));
				return false;
			}

			return true;
		}
		
		public void Dispose()
		{
			OnReturnRequested = null;
		}
		
		private async UniTask RefreshAsync()
		{
			var deckItemDatas = deckApplication.All.Select(deck =>
			{
				var ownedHero = User.OwnedVulcanites.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
				var heroData = gameDatabase.GetHero(ownedHero?.VulcaniteId);

				return new DuelDeckItemData
				{
					Id = deck.Id,
					Name = deck.Name,
					FactionUrl = $"{deck.Faction}_Flag",
					CountText = $"{deck.OwnedCardIds.Count}/{sharedConfig.MaxCardsInDeck}",
					LeagueFlagsUrls = LobbyBus.Leagues.Value
						.Where(league => league.IsDeckValid(deck))
						.Select(model => model.GetArtURL())
						.ToArray(),
					IsValid = deck.IsValid(),
					AvatarUrl = heroData?.ArtUrl,
					QuadrantUrl = $"{heroData?.Quadrant ?? Quadrant.Neutral}_Flag"
				};
			}).ToArray();
			
			var currDeck = deckApplication.Current ?? deckApplication.All.First();
			var selectedDeckId = currDeck.Id;


			Window.Setup(deckItemDatas)
				.SetSelectedAction(deckId =>
				{
					selectedDeckId = deckId;
					RefershSelectedAsync(selectedDeckId).Forget();
				})
				.SetCancelAction(() => CloseAsync().Forget())
				.SetSaveAction(() => Select(selectedDeckId).Forget());

			await RefershSelectedAsync(selectedDeckId);
		}

		private UniTask RefershSelectedAsync(string deckId)
		{
			if (!Validate(deckId))
				return UniTask.CompletedTask;
			
			var currDeck = deckApplication.All.First(x => x.Id == deckId);
			var ownedHero = User.OwnedVulcanites.FirstOrDefault(x => x.Id == currDeck.OwnedVulcaniteId);
			var heroData = gameDatabase.GetHero(ownedHero?.VulcaniteId);
			var avatarUrl = heroData?.ArtUrl;
			var frameUrl = customisationItemRepository.GetFirstEquipped(CustomisationType.AvatarFrame)?.PreviewURL;
			var heroName = heroData?.Name;
			var levelText = $"{heroData?.LevelAtSync.ToRoman() ?? "N/A"}";
			
			var description = gameDatabase
				.GetEffects(heroData?.EffectsIds)
				.Select(effectData => gameDatabase.GetKeyword(effectData.KeywordId).GetEffectDescription(effectData))
				.JoinToString("\n");

			return Window.SelectAsync(currDeck.Id, avatarUrl, frameUrl, description, heroName, levelText);
		}
		
		private void NotifyClientException(string message, string okText = "OK", Action repeatAction = null, bool cancel = false)
		{
			ConfirmationDialog.Instance.Init()
				.SetTitle("Information")
				.SetMessage(message)
				.SetResponseOk(repeatAction)
				.SetOk(okText)
				.SetCancel(cancel ? "Cancel" : null)
				.Apply();
		}
	}
}