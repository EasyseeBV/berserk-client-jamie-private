using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.UI;
using Events;
using Game;
using Lobby;
using Lobby.Items;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using Vulcan.Data;

namespace UI
{
	public partial class PlayerDecksView : BaseView
	{
		[SerializeField] private PlayerView playerView = default;
		
		private readonly List<IDisposable> disposables = new();
		private Dictionary<string, DeckButtonView> deckItems;
		private List<DeckData> deckModels;
		private string selectedDeck;
		
		protected override void OnAwake()
		{
			deckItems = new Dictionary<string, DeckButtonView>();
			deckModels = new List<DeckData>();
			DeleteButton.Subscribe(ConfirmDeleteDeck);
			BackButton.Subscribe(Close);
			EditButton.Subscribe(() =>
			{
				DeckCardListView.Instance.InitAndShow(GetCurrentDeckModel());
				Close();
			});
			NewButton.Subscribe(CreateNewDeck);
			
			EditButton.SetInteractable(false);
			DeleteButton.SetInteractable(false);
			
			SetActive(DeckButton, false);
			
			NewButton.SetHintTarget(TutorialTrigger.DeckNewBtn.ToString()).SetTransitionFactorSize().Init();
			EditButton.SetHintTarget(TutorialTrigger.DeckEditBtn.ToString()).SetTransitionFactorSize().Init()
				.Subscribe(EditButton.Get());
		}
		
		public void InitAndShow()
		{
			LobbyBus.OnUserDataRefreshed.SubscribeRaw(OnRentRefreshed);
			Init();
			Show();
		}
		
		private void Init()
		{
			deckItems.Clear();
			deckModels.Clear();
			DecksPanel.DestroyChildrenExcept(DeckButton.transform);
			deckModels.AddRange(DeckApplicationAdapter.Application.All);
			SetSelectedDeckFirst();
			foreach (var deck in deckModels)
			{
				var flags = LobbyBus.Leagues.Value
					.Where(league => league.IsDeckValid(deck))
					.Select(x => x.GetArtURL())
					.ToArray();
				
				var ownedHero = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
				var heroData = GameDataBaseAdapter.Instance.GetHero(ownedHero?.VulcaniteId);
				var deckItem = Instantiate(DeckButton, DecksPanel);
				deckItem.Id = deck.Id;
				deckItem.SetDeckName(deck.Name);
				deckItem.SetVulcaniteImage(heroData?.ArtUrl);
				deckItem.SetCoatImage($"{heroData?.Quadrant ?? Quadrant.Neutral}_Flag");
				deckItem.SetCardCount($"{deck.OwnedCardIds.Count}/{SharedConfigAdapter.Config.MaxCardsInDeck}");
				deckItem.SetLeagueFlags(flags);
				deckItem.SetFactionFlag($"{deck.Faction}_Flag");
				deckItem.SetSelectedAction(SelectDeck);
				SetActive(deckItem, true);
				deckItems.Add(deck.Id, deckItem);
			}

			SelectDeck(DeckApplicationAdapter.Application.Current?.Id ?? deckModels.FirstOrDefault()?.Id);
			OnRentRefreshed();
		}
		
		private void SetSelectedDeckFirst()
		{
			var currentId = DeckApplicationAdapter.Application.Current?.Id;
			if (string.IsNullOrEmpty(currentId))
				return;

			var currentDeck = deckModels.FirstOrDefault(x => x.Id == currentId);
			if (currentDeck == null)
				return;

			deckModels.Remove(currentDeck);
			deckModels.Insert(0, currentDeck);
		}
		
		private void OnRentRefreshed()
		{
			if (deckItems == null || deckItems.Count == 0 || deckModels == null || deckModels.Count == 0)
				return;
			
			foreach (var (deckId, deckItem) in deckItems)
			{
				var deckModel = deckModels.FirstOrDefault(x => x.Id == deckId);
				deckItem.SetWarning(!deckModel.IsValid());
			}
		}
		
		private void CreateNewDeck()
		{
			if (deckModels.Count >= SharedConfigAdapter.Config.MaxDeckCount)
			{
				ErrorDispatcher.OnInternalWarning.Publish(
					$"Can't create a new deck! Max allowed number of decks is {SharedConfigAdapter.Config.MaxDeckCount}");
				return;
			}
			
			var newDeckModel = new DeckData
			{
				Name = "New Deck",
				OwnedVulcaniteId = VulcaniteHandler.Owned.FirstOrDefault()?.Id,
				OwnedCardIds = new List<string>()
			};
			
			DeckCardListView.Instance.InitAndShow(newDeckModel);
			Close();
		}
		
		private void ConfirmDeleteDeck()
		{
			ConfirmationDialog.Instance.Init()
				.SetMessage(GameDataBaseAdapter.Instance.GetLocalization("DeckDelete"))
				.SetResponseOk(DeleteDeck)
				.Apply();
		}
		
		private async void DeleteDeck()
		{
			if (deckModels.Count == 1)
			{
				ErrorDispatcher.OnInternalWarning.Publish(GameDataBaseAdapter.Instance.GetLocalization("ErrorDeleteDeck"));
				return;
			}
			
			if (string.IsNullOrEmpty(selectedDeck))
				return;
			
			await DeckApplicationAdapter.Application.DeleteAsync(GetCurrentDeckModel());
			Init();
		}
		
		private DeckData GetCurrentDeckModel()
		{
			return deckModels.FirstOrDefault(x => x.Id == selectedDeck);
		}
		
		private void SelectDeck(string deckId)
		{
			selectedDeck = deckId;
			deckItems.Values.ForEach(x => x.SetSelect(x.Id == deckId));
			var valid = !string.IsNullOrEmpty(selectedDeck);
			SetDeckInfo();
			EditButton.SetInteractable(valid);
			DeleteButton.SetInteractable(valid);
		}
		
		private void SetDeckInfo()
		{
			if (string.IsNullOrEmpty(selectedDeck) || !deckModels.Exists(deck => deck.Id == selectedDeck))
			{
				RRLogger.Warning($"[{nameof(PlayerDecksView)}] - selected deck does not represented in collection");
				selectedDeck = DeckApplicationAdapter.Application.Current?.Id ?? deckModels.FirstOrDefault()?.Id;
			}

			if (string.IsNullOrEmpty(selectedDeck))
				return;
			
			var currentDeck = GetCurrentDeckModel();
			if (currentDeck == null)
				return;

			DeckTitleText.SetText(currentDeck.Name);
	
			var gameDataBase = GameDataBaseAdapter.Instance;
			var ownedHero = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == currentDeck.OwnedVulcaniteId);
			var heroData = gameDataBase.GetHero(ownedHero?.VulcaniteId);
			if (heroData == null)
			{
				playerView.SetArt(null);
				playerView.SetName("No Vulcanite");
				DescriptionContainer.gameObject.SetActive(false);
				LevelTxt.text = "-";
				EffectTxt.text = string.Empty;
				return;
			}
			playerView.SetArt(heroData.ArtUrl);
			playerView.SetName(heroData.Name);
			DescriptionContainer.gameObject.SetActive(heroData.EffectsIds.Any());
			LevelTxt.text = heroData.LevelAtSync.ToRoman();
			
			var effectsDescription = gameDataBase
				.GetEffects(heroData.EffectsIds)
				.Select(effectData => gameDataBase.GetKeyword(effectData.KeywordId).GetEffectDescription(effectData))
				.JoinToString("\n");
			
			EffectTxt.text = effectsDescription;
		}
		
		protected override void OnClosed()
		{
			base.OnClosed();
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
		}
		
		protected override void OnHidden()
		{
			base.OnHidden();
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
		}
		
		private void OnDestroy()
		{
			if (PlayerImage)
				PlayerImage.texture.DestroyImmediateSafe();
			
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
		}
	}
}
