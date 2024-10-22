using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.ChatWheel;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UI
{
	[Serializable]
	public class CustomisationPage : IDisposable
	{
		public CustomisationType Type;
		public RectTransform Container;
		public GameObject ItemPrefab;
		public Toggle Toggle;

		public void TriggerPageToggle(bool value)
		{
			if (Toggle)
				Toggle.onValueChanged?.Invoke(value);
		}

		public void SetActiveContent(bool value)
		{
			if (Container && Container.gameObject)
				Container.gameObject.SetActive(value);
		}

		public void Dispose()
		{
			if (Toggle)
				Toggle.onValueChanged.RemoveAllListeners();
		}
	}

	public class CustomisationPageView : PageView
	{
		[SerializeField] protected Button CloseButton;
		[SerializeField] protected Button SaveButton;
		[SerializeField] protected ScrollSnap TopBarScrollSnapView;
		[SerializeField] protected ScrollRect ContentsScrollView;
		[SerializeField] protected CustomisationPreviewView previewView;
		[SerializeField] protected List<CustomisationPage> Pages;

		private static readonly Color ACTIVE_COLOR = new Color(0.5345911f, 1f, 0.9430112f); // #88FFF0
		private static readonly Color INACTIVE_COLOR = Color.white;
		private ICustomisationViewFactory viewFactory;
		private CancellationTokenSource refreshPage;
		private CustomisationType selected;

		public override string Title => "Customisation";

		protected override void OnAwake()
		{
			base.OnAwake();
			viewFactory = new CustomisationViewFactory(Pages);
			// will setup the viewport, because it will change.
			var viewport = ContentsScrollView.viewport;
			viewport.anchorMin = Vector2.zero;
			viewport.anchorMax = Vector2.one;
			viewport.sizeDelta = Vector2.zero;
		}

		public override void InitAndShow()
		{
			selected = CustomisationType.None;
			Pages.ForEach(x => x.Toggle.onValueChanged.AddListener(_ => SetupPage(x)));
			Subscribe(CloseButton, () =>
			{
				CancelChanges();
				SentRequestSwitchPage();
			});
			Subscribe(SaveButton, () => SaveChanges().Forget());
			previewView.ResetPreview();
			var page = Pages.FirstOrDefault(x=> x.Toggle.isOn) ?? Pages.First();
			page.TriggerPageToggle(true);
			ScrollToPageAsync(page).Forget();
			Show(noAnimation: true);
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}
		public override bool CanDropIn(BaseView draggedView)
		{
			if (draggedView is not IChatWheelElement chatWheelElement)
				return false;

			return chatWheelElement.OccupiedSlot != null;
		}

		public override void DropIn(BaseView draggedView)
		{
			var chatWheelElement = draggedView as IChatWheelElement;
			previewView.ReleseChatWheelElement(chatWheelElement);
			chatWheelElement?.Dispose();
		}

		private async UniTask ScrollToPageAsync(CustomisationPage page)
		{
			await UniTask.Yield(PlayerLoopTiming.PostLateUpdate); // need to be ScrollSnap init
			var pageIndex = page.Toggle.transform.GetSiblingIndex();
			TopBarScrollSnapView.ChangePage(pageIndex);
		}

		private void SetupPage(CustomisationPage page)
		{
			page.Toggle.targetGraphic.color = page.Toggle.isOn
				? ACTIVE_COLOR
				: INACTIVE_COLOR;

			if (!page.Toggle.isOn)
			{
				CleanupPageContent(page);
				RefreshLobbyMusicState();
				return;
			}

			if (selected == page.Type)
				return;
			
			refreshPage?.Cancel();
			refreshPage?.Dispose();
			refreshPage = new CancellationTokenSource();
			selected = page.Type;
			SetupScrollView(page);
			SetupPageAsync(refreshPage.Token).Forget();
		}

		private void CleanupPageContent(CustomisationPage page)
		{
			var items = page.Container.GetComponentsInChildren<ICustomisationItemView>();
			items.ForEach(x=> x.Dispose());
		}

		private void SetupScrollView(CustomisationPage page)
		{
			ContentsScrollView.content = page.Container;
		}

		private async UniTask SetupPageAsync(CancellationToken token = default)
		{
			if (selected == CustomisationType.None)
				return;

			Pages.ForEach(x => x.SetActiveContent(x.Type == selected));
			CustomisationServiceAdapter.Repository.Get().ForEach(x => x.Clear()); // cleanup old usage

			var items = CustomisationServiceAdapter.Repository.Get(selected).ToArray();
			var pageItemViews = await UniTask.WhenAll(items.Select(x => GetCustomisationViewAsync(x, token)));
			token.ThrowIfCancellationRequested();

			if (selected == CustomisationType.Emotions)
			{
				var equppedItems = items.Where(x => x.IsEquipped).ToArray();
				var initTasks = equppedItems.Select(x => GetEmotionItemViewAsync(x, token, DragMode.Move, previewView.transform));
				var emotionViews = await UniTask.WhenAll(initTasks);
				var chatWheelElemens = emotionViews.OfType<IChatWheelElement>().ToArray();
				await previewView.SetupChatWheelAsync(chatWheelElemens, token);
				return;
			}

			token.ThrowIfCancellationRequested();
			var equppedItemView = pageItemViews.FirstOrDefault(x => x.Current.IsEquipped);
			await SetupPreview(equppedItemView, token);
		}

		private async UniTask<ICustomisationItemView> GetEmotionItemViewAsync(
			CustomisationItem item,
			CancellationToken token = default,
			DragMode? dragMode = null,
			Transform parent = null)
		{
			if (token.IsCancellationRequested)
				return default;

			dragMode ??= item.IsEquipped ? DragMode.None : DragMode.Copy;
			var itemView = viewFactory.Create<ICustomisationDraggableView>(item.CustomisationType, parent);
			itemView.SetDragMode(dragMode.Value);
			await itemView.SetupAsync(item, token);
			return itemView;
		}

		private async UniTask<ICustomisationItemView> GetCustomisationViewAsync(
			CustomisationItem item,
			CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();
			var type = item.CustomisationType;
			var isEmotions = type is CustomisationType.Emotions;
			var itemView = isEmotions 
				? await GetEmotionItemViewAsync(item, token) 
				: viewFactory.Create(type);

			item.OnUnequipped += DeselectItem;
			item.OnEquipped += SelectItem;
			
			if (!isEmotions)
				await itemView.SetupAsync(item, token);
			
			token.ThrowIfCancellationRequested();

			switch (item.IsEquipped)
			{
				case true: itemView.Select(); break;
				case false: itemView.Deselect(); break;
			}

			return itemView;

			void DeselectItem (string id)
			{
				itemView?.Deselect();
				
				// when deselect emotion will unlock drag mode
				if (itemView is ICustomisationDraggableView draggableView)
					draggableView.SetDragMode(DragMode.Copy);
			}

			void SelectItem(string id)
			{
				itemView?.Select();
				
				// when selected emotion will lock drag mode
				if (itemView is IChatWheelElement {OccupiedSlot: null} and ICustomisationDraggableView draggable)
					draggable.SetDragMode(DragMode.None);
				
				if (isEmotions || type == CustomisationType.None)
					return;

				CustomisationServiceAdapter.Application.UnequipItemExept(id, itemView?.Current?.CustomisationType);
				SetupPreview(itemView, token).Forget();
			}
		}

		private UniTask SetupPreview(ICustomisationItemView view, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (view is ICustomisationPreviewableView previewableView)
				return previewView.SetupAsync(previewableView.GetPreviewInfo(), token);

			previewView.ResetPreview();
			return UniTask.CompletedTask;
		}

		private async UniTask SaveChanges()
		{
			try
			{
				await CustomisationServiceAdapter.Application.AcceptChanges().AddLoadingTask();
			}
			catch (Exception e)
			{
				RRLogger.Error(e, "Changes will be cancelled");
				CancelChanges();
			}
		}

		private void CancelChanges()
		{
			CustomisationServiceAdapter.Application.CancelChanges();
			RefreshLobbyMusicState();
		}

		private void RefreshLobbyMusicState()
		{
			CustomisationServiceAdapter.Application.UpdateMainThemeMusic(CustomisationType.LobbyMusic);
		}

		private void Clear()
		{
			CloseButton.onClick.RemoveAllListeners();
			SaveButton.onClick.RemoveAllListeners();
			CustomisationServiceAdapter.Repository.Get().ForEach(x => x.Clear());
			previewView.ResetPreview();
			
			foreach (var page in Pages)
			{
				CleanupPageContent(page);
				page.Dispose();
			}
		}
	}
}