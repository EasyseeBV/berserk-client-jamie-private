using RR.UI.FrameSystem;
using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public partial class DeckBuilderCardView : DeckItemView, IPreviewable
	{
		[SerializeField] private float scale = 0.7f;

		private Action onDropEnded;
		private bool isDragging;
		private LoopScrollRect scroll;
		private CancellationTokenSource updateArt;
		private Transform self;
		
		protected override void Start()
		{
			base.Start();
			PreviewSystemAdapter.Instance.Registration(this);
		}
		
		public override void SetScrollData(int index, IDeckCardStack deckCardStack, params object[] other)
		{
			var selfInstance = this;
			if (!selfInstance)
				return;
			
			self = transform;
			scroll = other.OfType<LoopScrollRect>().FirstOrDefault();
			base.SetScrollData(index, deckCardStack, other);
		}

		protected override void OnRefreshView(IDeckCardStack deckCardStack)
		{
			var selfInstance = this;
			if (!selfInstance)
				return;
			
			if (deckCardStack?.CardData == null || deckCardStack.Count == 0)
				return;
			
			base.OnRefreshView(deckCardStack);
			
			InterruptUpdatingArt();
			updateArt = new CancellationTokenSource();
			HandCardArtView
				.SetupAsync(deckCardStack.CardData.ToCardDataAdapter(), updateArt.Token)
				.ContinueWith(() =>
				{
					HandCardArtView.SetWarning(!deckCardStack.AnyInStackValid);
					HandCardArtView.SetTransparency(deckCardStack.AnyInStackValid ? 1 : 0.5f);
					HandCardArtView.SetActiveShine(false);
					HandCardArtView.SetActive(true);
					ResetDrag();
				})
				.Forget();
		}

		private OwnedCard StackSelector(OwnedCard[] stack)
		{
			return stack?.FirstOrDefault(DeckCardStack.ValidateCard) ?? stack?.FirstOrDefault();
		}
		
		private void SetActiveCounter(bool value)
		{
			if (CounterArt)
				SetActive(CounterArt, value);
		}

		private void SetCount(int count)
		{
			SetActiveCounter(count > 1);
			
			if (CounterTxt)
				Set(CounterTxt, $"x{count}");

			SetDragMode(count);
		}

		private void InterruptUpdatingArt()
		{
			updateArt?.Cancel();
			updateArt?.Dispose();
			updateArt = null;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			InterruptUpdatingArt();
			if (HandCardArtView)
				HandCardArtView.SetActive(false);
			SetActiveCounter(false);
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			PreviewSystemAdapter.Instance.UnRegistration(this);
			InterruptUpdatingArt();
			onDropEnded = null;
			scroll = null;
		}

		#region Drag & Drop

		public override void OnDropped(BaseView acceptor)
		{
			base.OnDropped(acceptor);
			DeckCardStack.RequestToRemove(DeckCardStack.Get(StackSelector));
			onDropEnded?.Invoke();
			onDropEnded = null;
			Destroy(gameObject); // destroy this view after OnDropped
		}

		public override void OnDropCancelled()
		{
			base.OnDropCancelled();
			// BaseView destroy this view after OnDropCancelled
			onDropEnded?.Invoke();
			onDropEnded = null;
		}

		public override void OnStartDrag()
		{
			// when last card does not created a new copy of this card then will setup a card in this method
			if (DeckCardStack.Count == 1) 
				SetupDrag(ResetDrag);

			isDragging = true;
			PreviewSystemAdapter.Instance.Close();
			base.OnStartDrag();
		}

		public override BaseView Clone()
		{
			// pool not working with dragDop & create a new empty element that does not duplicate the texture instance
			var view = Instantiate(scroll.PrefabSource.Prefab, transform.parent).GetComponent<DeckBuilderCardView>();
			var isClone = true;
			view.SetScrollData(0, DeckCardStack, scroll, isClone);
			view.SetupDrag(ResetDrag);
			
			SetCount(DeckCardStack.Count - 1); // to visual only
			SetDragMode(DeckCardStack.Count - 1); // switch drag mode by count in stack
			return view;
		}

		private void SetupDrag(Action onComplete)
		{
			self.localScale = Vector3.one;
			scroll.Vertical = false;
			onDropEnded += () => onComplete?.Invoke();
		}

		private void ResetDrag()
		{
			isDragging = false;
			self.localScale = Vector3.one * scale;
			scroll.Vertical = true;
			var countInStack = IsClone || DeckCardStack == null ? 1 : DeckCardStack.Count;
			SetCount(countInStack);
			SetDragMode(countInStack);
		}

		private void SetDragMode(int count)
		{
			if (DeckCardStack is {AnyInStackValid: false})
			{
				DragMode = DragMode.None;
				return;
			}
			
			DragMode = count > 1
				? DragMode.Copy
				: DragMode.Move;
		}

		#endregion

		#region Previewable

		public GameObject TargetView => gameObject;

		public IPreviewData PreviewData => DeckCardStack?.CardData?.ToPreviewData();
		public IPreviewSetting PreviewSettings { get; } = new PreviewSettings(PreviewType.Fit);
		public bool CanPreview() => !IsClone && !isDragging && TargetView && DeckCardStack?.CardData != null;

		#endregion
	}
}