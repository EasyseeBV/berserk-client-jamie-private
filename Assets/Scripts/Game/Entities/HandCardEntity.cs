using System.Linq;
using DG.Tweening;
using Events;
using Game.Effect_System.Target;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UI;
using UnityEngine.EventSystems;
using Vulcan.Data;
using Vulcan.Network;

namespace Game.Entities
{
	public class HandCardEntity : MonoEntityBase<CardData>, IPointerClickHandler
	{
		private Tweener fadeTween;
		private readonly HandService handService = new HandService(); //TODO: DI

		public bool IsDestroyed { get; private set; }

		public bool CanPlaceSpell()
		{
			return Data.EffectsContainer.Effects.All(effect => TargetResolver.ConditionHandler.IsSpellPlayAllowed(this, effect));
		}

		public void SetTurn(bool myTurn)
		{
			//Cards without a HandCardView are all cards that are not in the player's hand
			if (!(View is HandCardView handCardView))
				return;

			handCardView.OnTurnChanged();
		}

		protected void Start()
		{
			//Cards without a HandCardView are all cards that are not in the player's hand
			//TODO: make check more obvious and secure
			if (!(View is HandCardView handCardView))
				return;

			handCardView.SetDropCallback(TryDropCard);

			GameBus.OnSpawnCanceled
				.Subscribe(this, x => x.Equals(Data), () =>
				{
					fadeTween?.Kill(true);
					fadeTween = handCardView.CanvasGroup.DOFade(1f, 1f);
					handCardView.ChangeState();
				}).CallWhenInactive();

			GameBus.OnSpawnConfirmed
				.Subscribe(this, x => x.Equals(Data), () => handService.RemoveCard(this))
				.CallWhenInactive();

			GameBus.OnContextUpdated.Subscribe(this, RefreshView);
			View.SetUp(Data);
		}

		private void RefreshView()
		{
			View.SetUp(Data);
		}

		public void Discard()
		{
			if (IsDestroyed)
				return;

			View.gameObject.SetActive(false);

			IsDestroyed = true;
			RectTransform.parent = null;
			Destroy();
		}

		private bool TryDropCard()
		{
			if (Data.Type == CardType.Spell && !CanPlaceSpell())
			{
				GameBus.OnActionBlocked += BlockedInfo.NoSpellTarget;
				return false;
			}

			fadeTween?.Kill(true);
			fadeTween = View.CanvasGroup.DOFade(0f, 2f);
			View.CanvasGroup.blocksRaycasts = false;
			CommandController.Enqueue(TrySpawnCard);

			var isAwaitDropConfirm = Data.EffectsContainer.Effects.Any(e => e.IsManualPick);
			return !isAwaitDropConfirm;

			void TrySpawnCard()
			{
				View.CanvasGroup.blocksRaycasts = true;

				if (Data.Lava > GameBus.LocalContext.GetVulcaniteByOwner(Data.Owner).Data.Lava)
				{
					GameBus.OnActionBlocked += BlockedInfo.NotEnoughLava;
					fadeTween?.Kill(true);
					fadeTween = View.CanvasGroup.DOFade(1f, 1f);
					View.OnDropCancelled();
					return;
				}

				GameBus.OnSpawnCard += Data;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			RRLogger.Log($"{Data.Title.Red()}_{Data.UID} {Data.EffectsContainer.ToString().Green()}");
		}
	}
}