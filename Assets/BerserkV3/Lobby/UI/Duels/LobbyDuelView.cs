using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Duels
{
	public partial class LobbyDuelView : SafeView
	{
		[SerializeField] protected LobbyDuelItemView Prototype;
		[SerializeField] protected TMP_InputField SearchInput;

		private readonly List<LobbyDuelItemView> roomItemViews = new();
		private Func<string, string, bool> filterFunction = (roomId, _) => !string.IsNullOrEmpty(roomId);
		private bool releasePrevious;

		public async UniTask BuildAsync(string userName, string heroArtUrl, string frameArtUrl,
			CancellationToken token = default)
		{
			NameText.SetText(userName);
			await UniTask.WhenAll(
					AvatarImage.LoadResourceAsync(heroArtUrl, token, releasePrevious), 
					FrameImage.LoadResourceAsync(frameArtUrl, token, releasePrevious))
				.AttachExternalCancellation(token);

			releasePrevious = true;
			SearchInput.onValueChanged.RemoveAllListeners();
			SearchInput.onValueChanged.AddListener(ApplyFilter);
		}

		public void AddOrRefresh(DuelRoomItemData value)
		{
			var instanceView = roomItemViews.FirstOrDefault(x => x.Id == value.Id);
			if (!instanceView)
			{
				instanceView = Instantiate(Prototype, RoomsContainer);
				roomItemViews.Add(instanceView);
			}

			instanceView.Setup(value);
		}

		public void Delete(DuelRoomItemData value)
		{
			Delete(roomItemViews.FirstOrDefault(x => x.Id == value?.Id));
		}

		private void Delete(LobbyDuelItemView view)
		{
			if (!view)
				return;

			roomItemViews.Remove(view);
			view.Clear();
			view.DestroyGameObject();
		}

		public LobbyDuelView SubscribeOnCreateRoom(Action value)
		{
			CreateButton.onClick.RemoveAllListeners();
			CreateButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public LobbyDuelView SubscribeOnJoinRoom(Action value)
		{
			JoinRoomButton.onClick.RemoveAllListeners();
			JoinRoomButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public LobbyDuelView SubscribeOnSelectDeck(Action value)
		{
			SelectDeckButton.onClick.RemoveAllListeners();
			SelectDeckButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public LobbyDuelView SubscribeOnPractice(Action value)
		{
			PracticeButton.onClick.RemoveAllListeners();
			PracticeButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public LobbyDuelView SubscribeOnBack(Action value)
		{
			BackButton.onClick.RemoveAllListeners();
			BackButton.onClick.AddListener(() => value?.Invoke());
			return this;
		}

		public LobbyDuelView SetFilter(Func<string, string, bool> value)
		{
			filterFunction = value ?? ((roomId, _) => !string.IsNullOrEmpty(roomId));
			return this;
		}

		public void Clear()
		{
			roomItemViews.Where(view => view && view.gameObject).ForEach(view =>
			{
				view.Clear();
				view.DestroyGameObject();
			});
			roomItemViews.Clear();
			SetFilter(null);
			if (releasePrevious)
			{
				FrameImage.ReleaseResource();
				AvatarImage.ReleaseResource();
			}

			releasePrevious = false;
			if (CreateButton)
				CreateButton.onClick.RemoveAllListeners();

			if (JoinRoomButton)
				JoinRoomButton.onClick.RemoveAllListeners();

			if (SelectDeckButton)
				SelectDeckButton.onClick.RemoveAllListeners();

			if (PracticeButton)
				PracticeButton.onClick.RemoveAllListeners();

			if (BackButton)
				BackButton.onClick.RemoveAllListeners();

			if (SearchInput)
				SearchInput.onValueChanged.RemoveAllListeners();
		}

		public void ApplyFilter()
		{
			if (SearchInput)
				ApplyFilter(SearchInput.text);
		}

		private void ApplyFilter(string query)
		{
			roomItemViews.ForEach(view => view.SetActive(filterFunction.Invoke(view.Id, query?.Trim().ToLower())));
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