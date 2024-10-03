using System;
using System.Collections.Generic;
using System.Threading;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Duels
{
	public partial class LobbyDuelSelectDeckView : SafeView
	{
		[SerializeField] private DeckButtonView prototype;
		private readonly List<DeckButtonView> deckButtonViews = new();
		private bool previousRelease;
		private event Action<string> OnSelected; 
		
		public LobbyDuelSelectDeckView Setup(IEnumerable<DuelDeckItemData> deckItemDatas)
		{
			deckButtonViews.ForEach(x => x.DestroyGameObject());
			deckButtonViews.Clear();
			foreach (var data in deckItemDatas)
			{
				var deckItem = Instantiate(prototype, DecksLayout);
				deckItem.Id = data.Id;
				deckItem.SetDeckName(data.Name);
				deckItem.SetVulcaniteImage(data.AvatarUrl);
				deckItem.SetCoatImage(data.QuadrantUrl);
				deckItem.SetCardCount(data.CountText);
				deckItem.SetLeagueFlags(data.LeagueFlagsUrls);
				deckItem.SetFactionFlag(data.FactionUrl);
				deckItem.SetWarning(!data.IsValid);
				deckItem.SetSelectedAction(id => OnSelected?.Invoke(id));
				deckItem.SetInteractable(data.IsValid);
				SetActive(deckItem, true);
				deckButtonViews.Add(deckItem);
			}

			return this;
		}

		public async UniTask SelectAsync(
			string deckId,
			string avatarUrl,
			string frameUrl,
			string description,
			string heroName,
			string heroLevel,
			CancellationToken token = default)
		{
			await UniTask.WhenAll(
				AvatarImage.LoadResourceAsync(avatarUrl, token, previousRelease),
				FrameImage.LoadResourceAsync(frameUrl, token, previousRelease))
				.AttachExternalCancellation(token);
			
			previousRelease = true;
			LevelText.SetText(heroLevel);
			NameText.SetText(heroName);
			SetActive(DescriptionContainer, !string.IsNullOrEmpty(description));
			DescriptionText.SetText(description);
			deckButtonViews.ForEach(x => x.SetSelect(x.Id == deckId));
		}

		public LobbyDuelSelectDeckView SetSelectedAction(Action<string> value)
		{
			OnSelected += value;
			return this;
		}

		public LobbyDuelSelectDeckView SetSaveAction(Action value)
		{
			SaveButton.onClick.RemoveAllListeners();
			SaveButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public LobbyDuelSelectDeckView SetCancelAction(Action value)
		{
			BackButton.onClick.RemoveAllListeners();
			BackButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public void Clear()
		{
			if (previousRelease)
			{
				AvatarImage.ReleaseResource();
				FrameImage.ReleaseResource();
			}

			deckButtonViews.ForEach(x => x.DestroyGameObject());
			deckButtonViews.Clear();
			
			NameText.SetText("");
			LevelText.SetText("");
			DescriptionText.SetText("");
			previousRelease = false;
			OnSelected = null;
		}
		
		private void OnDestroy()
		{
			Clear();
		}
		
		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}
	}
}