using DG.Tweening;
using Events;
using Game.Entities;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using System.Linq;
using Berserk.Shared.Data.Enums;
using UnityEngine;
using Vulcan.Data;
using CardData = Vulcan.Data.CardData;

namespace UI
{
	[RequireComponent(typeof(TableCardFactory))]
	public partial class TableActiveZonePanel : BaseView
	{
		[SerializeField] private float cardsSpacing = 14f;

		private TableCardFactory tableCardFactory;

		protected override void OnAwake()
		{
			GameBus.LocalContext.OnTableChanged += ReorderCardsOnTable;
			GameBus.OnEntityDie.Subscribe(this, x => x is CardData, OnDead);
			TryGetComponent(out tableCardFactory);
		}

		private void OnDestroy()
		{
			GameBus.LocalContext.OnTableChanged -= ReorderCardsOnTable;
			GameBus.OnEntityDie.Unsubscribe(OnDead);
		}

		public override bool CanDropIn(BaseView draggedView)
		=> draggedView is HandCardView handCardView
		&& (!tableCardFactory.IsMaxOwnedCardsOnTable(handCardView.Data));

		private void ReorderCardsOnTable(Owner owner)
		{
			var upOrDown = owner == Berserk.Shared.Data.Enums.Owner.Self ? -1 : 1;
			var width = GameBus.LocalContext
				.GetTableCardsByOwner(owner)
				.Skip(1)
				.Sum(x => x.RectTransform.sizeDelta.x + cardsSpacing) / 2f;

			var originPosition = tableCardFactory.TableContent.position + new Vector3(-width, upOrDown * tableCardFactory.TableContent.sizeDelta.y / 4f, -100);

			GameBus.LocalContext
				.GetTableCardsByOwner(owner)
				.OrderBy(x => x.transform.position.x) // determines last drop position
				.ForEach((card, i) => card.SetPosition(originPosition + GetDeltaPosition(card, i)));

			Vector3 GetDeltaPosition(IMonoEntity entity, int step)
			{
				return new Vector2((cardsSpacing + entity.RectTransform.sizeDelta.x) * step, 0);
			}
		}

		private void OnDead(DataBase data)
		{
			var entity = GameBus.LocalContext
				.GetTableCardsByOwner(data.Owner)
				.FirstOrDefault(x => x.DataBase == data);

			var destructionDelay = entity.Data.EffectsContainer.RequiredDelayDestruction
				? 5f
				: 0f;

			DOVirtual.DelayedCall(destructionDelay, () => GameBus.LocalContext.RemoveTableCard(entity));
		}
	}
}