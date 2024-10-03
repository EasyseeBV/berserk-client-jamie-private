using System;
using Audio;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Vulcan.Audio;
using Vulcan.Data;

namespace UI
{
	[Obsolete]
	public partial class HandCardView : MonoEntityBaseView, IPointerEnterHandler, IPointerExitHandler
	{
		private const float ANIM_DURATION = 0.35f;
		[SerializeField] private float SelectScale = 1.1f;
		[SerializeField] private float IdleScale = 0.8f;

		public CardData Data;

		private Tween hoverTween;
		private Vector2 position;
		private Quaternion rotation;
		private Transform initialParent;
		private int siblingIndex;

		private bool isDragging;
		private bool setUp;

		private Func<bool> onDropped;

		protected override void OnAwake()
		{
			base.OnAwake();
			Init();
		}

		public HandCardView Init()
		{
			transform.localScale = Vector3.one * IdleScale; // setup default scale
			SetShine(false);
			return this;
		}

		public override void SetUp(DataBase data)
		{
			if (!setUp)
			{
				GameBus.TargetSelecting.Subscribe(this, ChangeState);
				GameBus.OnPassingTurned.Subscribe(this, SetAsUndraggable);
				GameBus.OnLavaChanged.Subscribe(this,
					(x, y) => x == Data.Owner,
					(x, y) => ChangeState());
			}

			setUp = true;

			UpdateValues((CardData)data);

			data.Attack.OnChanged += x => HandCardArtView.SetAttackText(x);
			data.Hp.OnChanged += x => HandCardArtView.SetHealthText(x);
			data.Lava.OnChanged += x => HandCardArtView.SetManaText(x);

			ChangeState();
		}

		public void SetWarning(bool value)
		{
			HandCardArtView.SetWarning(value);
		}

		public HandCardView UpdateValues(CardData data)
		{
			Data = data;
			return this;
		}

		public override void SetPosition(Vector3 p, Quaternion r)
		{
			(position, rotation) = (p, r);
		}

		public void SetDefaultPosition()
		{
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
		}

		public void SetDropCallback(Func<bool> onDropped)
		{
			this.onDropped = onDropped;
		}

		public override void OnDropped(BaseView acceptor)
		{
			if (!onDropped.Invoke())
			{
				OnDropCancelled();
				return;
			}

			base.OnDropped(acceptor);
			isDragging = false;
		}

		public override void OnDropCancelled()
		{
			transform.SetParent(initialParent);
			transform.SetSiblingIndex(siblingIndex);

			isDragging = false;
			ResetPosition();

			AudioController.Play(Clip.Card_Release);
		}

		public override void OnStartDrag()
		{
			initialParent = transform.parent;
			siblingIndex = transform.GetSiblingIndex();
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one * 1.5f;
			FullCardView.Hide();
			hoverTween?.Kill();
			isDragging = true;

			base.OnStartDrag();

			AudioController.Play(Clip.Card_StartDrag);
		}

		public void ChangeState()
		{
			var interactable = setUp
			                   && GameBus.TargetSelecting.Value == false
			                   && GameBus.CurrentRound.Value.TurnOwner == Berserk.Shared.Data.Enums.Owner.Self
			                   && Data.Lava <= GameBus.LocalContext.GetVulcaniteByOwner(Data.Owner)?.Data.Lava;

			DragMode = interactable
				? DragMode.Move
				: DragMode.None;

			CanvasGroup.interactable = interactable;
			SetShine(interactable);
		}

		public void SetShine(bool value)
		{
			if(HandCardArtView)
				HandCardArtView.SetActiveShine(value);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			//TODO: Extract general logic for displaying the card in different situations (graveyard, hand, deck builder, full view)
			if (isDragging || eventData.dragging)
				return;

			Select(Color.white);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			//TODO: Extract general logic for displaying the card in different situations (graveyard, hand, deck builder, full view)
			if (isDragging)
				return;

			Unselect();
		}

		public override void Select(Color selectedColor)
		{
			AudioController.Play(Clip.Card_Hover);
			hoverTween = transform.DOScale(SelectScale, ANIM_DURATION);
			FullCardView.SetPosition();
			// FullCardView.Show(Data);
		}

		public override void Unselect()
		{
			FullCardView.Hide();
			ResetPosition();
		}

		public override void SelectOnRequestingTarget(Color requestingColor)
		{
		}

		public void OnTurnChanged()
		{
			ChangeState();

			TryCancelDragging();
		}

		private void ResetPosition()
		{
			hoverTween?.Kill();
			if (position != Vector2.zero)
				hoverTween = DOTween.Sequence()
					.Join(transform.DOLocalMoveY(position.y, ANIM_DURATION / 2f))
					.Join(transform.DOLocalMoveX(position.x, ANIM_DURATION / 2f))
					.Join(transform.DOLocalRotateQuaternion(rotation, ANIM_DURATION / 2f))
					.Join(transform.DOScale(IdleScale, ANIM_DURATION / 2f))
					.Play();
			else
				hoverTween = transform.DOScale(IdleScale, ANIM_DURATION / 2f);
		}

		public void SetAsUndraggable()
		{
			if(!CanvasGroup)
				return;
			
			DragMode = DragMode.None;
			CanvasGroup.interactable = false;
			SetShine(false);
			TryCancelDragging();
		}

		private void TryCancelDragging()
		{
			if (UIManager.Dragger.IsDragging)
				UIManager.Dragger.CancelDragDrop();
		}
	}
}