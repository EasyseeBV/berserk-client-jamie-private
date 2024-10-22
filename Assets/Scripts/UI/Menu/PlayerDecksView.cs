using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.UserInventory;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Decks;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.UI;
using BerserkV3.Startup.Authorization.Inventory.Models;
using Lobby;
using Lobby.Items;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using Zenject;

namespace UI
{
	[Obsolete]
	//TODO check and remove // will be removed during deck rework
	public partial class PlayerDecksView : BaseView
	{
		[SerializeField] private PlayerView playerView = default;
		
		private readonly List<IDisposable> disposables = new();
		private Dictionary<string, DeckButtonView> deckItems;
		private List<OwnedDeck> deckModels;
		private string selectedDeck;
		

		protected override void OnAwake()
		{
			deckItems = new Dictionary<string, DeckButtonView>();
			deckModels = new List<OwnedDeck>();
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
			Debug.LogError("Class sholud be replaced");
			/*deckItems.Clear();
			deckModels.Clear();
			DecksPanel.DestroyChildrenExcept(DeckButton.transform);
			deckModels.AddRange(DeckApplicationAdapter.Application.AllDecks);
			SetSelectedDeckFirst();
			foreach (var deck in deckModels)
			{
				var flags = LobbyBus.Leagues.Value
					.Where(league => league.IsDeckValid(deck))
					.Select(x => x.GetArtURL())
					.ToArray();
				
				var ownedHero = userInventory.OwnedVulcanites.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
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
			
			SelectDeck(DeckApplicationAdapter.Application.Current.Id);
			OnRentRefreshed();*/
		}
		
		private void SetSelectedDeckFirst()
		{
			var currentDeck = deckModels.FirstOrDefault(x => x.Id == DeckApplicationAdapter.Application.Current.Id);
			deckModels.Remove(currentDeck);
			deckModels.Insert(0, currentDeck);
		}
		
		private void OnRentRefreshed()
		{
			/*if (deckItems == null || deckItems.Count == 0 || deckModels == null || deckModels.Count == 0)
				return;
			
			foreach (var (deckId, deckItem) in deckItems)
			{
				var deckModel = deckModels.FirstOrDefault(x => x.Id == deckId);
				deckItem.SetWarning(!deckModel.IsValid());
			}*/
		}
		
		private void CreateNewDeck()
		{
			if (deckModels.Count >= SharedConfigAdapter.Config.MaxDeckCount)
			{
				ConfirmationDialog.Instance.Init()
					.SetMessage($"Can't create a new deck! Max allowed number of decks is {SharedConfigAdapter.Config.MaxDeckCount}")
					.SetTitle("Information")
					.SetCancel()
					.Apply();
				return;
			}
			
			var newDeckModel = new OwnedDeck
			{
				Name = "New Deck",
				//OwnedVulcaniteId = userInventory.OwnedVulcanites.FirstOrDefault()?.Id,
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
				ConfirmationDialog.Instance.Init()
					.SetMessage(GameDataBaseAdapter.Instance.GetLocalization("ErrorDeleteDeck"))
					.SetTitle("Information")
					.SetCancel()
					.Apply();
				
				return;
			}
			
			if (string.IsNullOrEmpty(selectedDeck))
				return;
			
			await DeckApplicationAdapter.Application.DeleteAsync(GetCurrentDeckModel());
			Init();
		}
		
		private OwnedDeck GetCurrentDeckModel()
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
			/*if (string.IsNullOrEmpty(selectedDeck) || !deckModels.Exists(deck => deck.Id == selectedDeck))
			{
				RRLogger.Warning($"[{nameof(PlayerDecksView)}] - selected deck does not represented in collection");
				selectedDeck = DeckApplicationAdapter.Application.Current.Id;
			}
			
			var currentDeck = GetCurrentDeckModel();
			DeckTitleText.SetText(currentDeck.Name);

			var gameDataBase = GameDataBaseAdapter.Instance;
			var ownedHero = userInventory.OwnedVulcanites.FirstOrDefault(x => x.Id == currentDeck.OwnedVulcaniteId);
			var heroData = gameDataBase.GetHero(ownedHero?.VulcaniteId);
			playerView.SetArt(heroData.ArtUrl);
			playerView.SetName(heroData.Name);
			DescriptionContainer.gameObject.SetActive(heroData.EffectsIds.Any());
			LevelTxt.text = heroData.LevelAtSync.ToRoman();
			
			var effectsDescription = gameDataBase
				.GetEffects(heroData.EffectsIds)
				.Select(effectData => gameDataBase.GetKeyword(effectData.KeywordId).GetEffectDescription(effectData))
				.JoinToString("\n");
			
			EffectTxt.text = effectsDescription;*/
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
				PlayerImage.ReleaseResource();
			
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
		}
	}
}