using System.Threading;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Generic.ChatWheel;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class CustomisationPreviewView : BaseView
	{
		[SerializeField] protected RectTransform PreviewLayout;
		[SerializeField] protected RawImage PreviewMask;
		[SerializeField] protected RawImage AdditionImage;
		[SerializeField] protected RawImage PreviewImage;
		[SerializeField] protected TextMeshProUGUI TitleText;
		[SerializeField] protected AspectRatioFitter PreviewAspect;
		[SerializeField] protected ChatWheelView ChatWheelView;
		[SerializeField] protected int ChatWheelСapacity = 8;

		public void ResetPreview()
		{
			ChatWheelView.Clear();
			ChatWheelView.Close(force: true);
			Set(TitleText, string.Empty);
			SetActive(PreviewLayout, false);
			SetActive(PreviewMask, false);
			SetActive(TitleText, false);
			AdditionImage.ReleaseResource();
			PreviewMask.ReleaseResource();
			PreviewImage.ReleaseResource();
		}
		
		public async UniTask SetupAsync(PreviewInfo info, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();
			ResetPreview();

			var isMusicContent = info.Type is CustomisationType.LobbyMusic or CustomisationType.BattleMusic;
			var previewTask = isMusicContent ? UniTask.CompletedTask : LoadArt(PreviewImage, info.PreviewUrl, token);
			
			await UniTask.WhenAll(previewTask, 
				LoadArt(PreviewMask, info.PreviewMaskUrl, token), 
				LoadArt(AdditionImage, info.AdditionUrl, token));
			
			token.ThrowIfCancellationRequested();
			if (isMusicContent && CustomisationBus.OnMusicPlayTest.Value != info.PreviewUrl)
				CustomisationBus.OnMusicPlayTest += info.PreviewUrl;

			PreviewAspect.aspectRatio = info.PreviewAspect;
			Set(TitleText, info.TitleText);
			SetActive(PreviewLayout, true);
			SetActive(PreviewMask, true);
			SetActive(PreviewImage, !string.IsNullOrEmpty(info.PreviewUrl) && !isMusicContent);
			SetActive(AdditionImage, !string.IsNullOrEmpty(info.AdditionUrl));
			SetActive(TitleText, !string.IsNullOrEmpty(info.TitleText));
		}

		public UniTask SetupChatWheelAsync(IChatWheelElement[] items, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();
			ResetPreview();
			SetActive(PreviewLayout, true);
			
			ChatWheelView.Init(ChatWheelСapacity);
			ChatWheelView.Show(force:true);
			ChatWheelView.Set(items)?.ForEach(x => x?.Dispose());
			ChatWheelView.OnDroppedIn += DroppedInChatWheelSlot;

			return UniTask.CompletedTask;
		}

		public void ReleseChatWheelElement(IChatWheelElement element)
		{
			if (element?.OccupiedSlot == null)
				return;

			ChatWheelView.ReplaceSlotElement(element.OccupiedSlot.Id, null);
		}

		private UniTask LoadArt(RawImage target, string url, CancellationToken token)
		{
			return string.IsNullOrEmpty(url) 
				? UniTask.CompletedTask :
				target.LoadResourceAsync(url, token);
		}

		private void DroppedInChatWheelSlot(IChatWheelSlot targetSlot, IChatWheelElement dropped)
		{
			if (targetSlot.Element != null && dropped.OccupiedSlot != null)
			{
				ChatWheelView.SwapSlotsElement(targetSlot.Id, dropped.OccupiedSlot.Id);
				return;
			}
			
			if (dropped is ICustomisationDraggableView draggableView)
				draggableView.SetDragMode(DragMode.Move);
			
			var relesed = ChatWheelView.ReplaceSlotElement(targetSlot.Id, dropped);
			relesed?.Dispose();
		}

		private void OnDestroy()
		{
			if (ChatWheelView)
				ChatWheelView.Clear();
			
			AdditionImage.ReleaseResource();
			PreviewMask.ReleaseResource();
			PreviewImage.ReleaseResource();
		}
	}
}