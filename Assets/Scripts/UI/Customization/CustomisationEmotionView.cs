using System.Threading;
using BerserkV3.Generic.ChatWheel;
using BerserkV3.Generic.Customisation;
using BerserkV3.Generic.Emotions;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
	public class CustomisationEmotionView : EmotionView, IChatWheelElement, ICustomisationDraggableView
	{
		[SerializeField] protected GameObject SelectionImage;

		private bool isCopy;
		private bool disposed;
		private bool isVisible;
		private CancellationTokenSource copyUpdate;
		private ICustomisationViewFactory viewFactory;
		
		public CustomisationItem Current { get; private set; }

		public async UniTask SetupAsync(CustomisationItem item, CancellationToken token = default)
		{
			if (item.AssetData is not EmotionAssetData data)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] CustomisationItem.AssetData is not a ReactionAssetData");
				return;
			}
			
			Current = item;
			AutoDispose = false;
			disposed = false;
			token = copyUpdate?.Token ?? token;

			await InitAsync(new Emotion(item.Id, data.URL, data.Height, data.Width, data.TimeScale, data.LifeTime), token);
		}

		public void Deselect()
		{
			if (SelectionImage)
				SelectionImage.SetActive(false);
		}

		public void Select()
		{
			if (OccupiedSlot == null && SelectionImage && !disposed)
				SelectionImage.SetActive(true);
		}

		public void SetFactory(ICustomisationViewFactory factory)
		{
			viewFactory = factory;
		}

		public void SetDragMode(DragMode dragMode)
		{
			DragMode = dragMode;
		}
		
		public override void Hold(IChatWheelSlot slot)
		{
			base.Hold(slot);
			
			if (Current is {IsEquipped: false})
				Current.Equip();
		}

		public override void Release()
		{
			if (disposed)
			{
				base.Release();
				return;
			}

			if (Current is {IsEquipped: true})
				Current.Unequip();
			
			base.Release();
		}

		public override void Dispose()
		{
			copyUpdate?.Cancel();
			copyUpdate?.Dispose();
			copyUpdate = null;
			disposed = true;
			OccupiedSlot?.Set(null);
			Current = null;
			base.Dispose();
		}

		public override void OnStartDrag()
		{
			base.OnStartDrag();
			ResetSize();
		}

		public override BaseView Clone()
		{
			if (Current == null)
				return default;
			
			var itemView = viewFactory.Create<CustomisationEmotionView>(Current.CustomisationType, transform);

			itemView.isCopy = true;
			itemView.RectTransform.anchoredPosition = Vector2.zero;
			itemView.copyUpdate = new CancellationTokenSource();
			itemView.SetupAsync(Current).Forget();
			itemView.ResetSize();
			return itemView;
		}

		private void ResetSize()
		{
			if (!isCopy)
				return;
			
			// When drag the BaseView, will set parent the canvas without world position stays, this view have broken size
			transform.localScale = ProfileView.Instance.transform.localScale; 
		}
	}
}