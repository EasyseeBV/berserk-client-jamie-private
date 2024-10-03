using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.UI;
using BerserkV3.Startup.Authorization;
using DG.Tweening;
using RR.Core.DebugSystem;
using Zenject;
using Object = UnityEngine.Object;

namespace BerserkV3.GameCore.Repository
{
	public class GameRepository : DisposableWithCts, IGameRepository
	{
		private readonly IInstantiator instantiator;
		private readonly List<IHeroView> heroViews = new();
		private readonly List<ICardView> cardViews = new();
		private float tickTurnTransfer;
		
		public string SelfId { get; }
		public string OpponentId { get; private set; }
		public bool Initialized { get; private set; }

		public ICardData NextDeckCard { get; private set; }
		public IEnumerable<ICardView> CardViews => cardViews;
		public IEnumerable<IHeroView> HeroViews => heroViews;
		
		public GameRepository()
		{
			SelfId = User.Id;
			DOTween.SetTweensCapacity(500, 500);
		}
		
		public void Initialize(string opponentId)
		{
			OpponentId = opponentId;
			Initialized = true;
		}

		public void DeleteObjectByRuntimeId(int runtimeId)
		{
			var view = GetObjectViewByRuntimeId(runtimeId);
			UnRegisterRuntimeView(view);
			if(view != null)
				Object.Destroy(view.SelfContainer.gameObject);
		}

		public void RegisterCardView(ICardView cardView)
		{
			if (cardViews.Contains(cardView))
				return;

			cardViews.Add(cardView);
		}
		public void RegisterHeroView(IHeroView heroView)
		{
			if (heroViews.Contains(heroView))
			{
				RRLogger.Error("Twice attempt to register hero");
				return;
			}

			heroViews.Add(heroView);
		}

		public void UnRegisterRuntimeView(IRuntimeObjectView view)
		{ 
			cardViews.RemoveAll(x => x == view);
			heroViews.RemoveAll(x => x == view);
		}
		
		public void UpdateNextDeckCard(ICardData nextDeckCard)
		{
			NextDeckCard = nextDeckCard;
		}

		public ICardView GetCardViewByRuntimeId(int runtimeId)
		{
			return cardViews.FirstOrDefault(cardView => cardView.RuntimeData.Id == runtimeId);
		}
		public IRuntimeObjectView GetObjectViewByRuntimeId(int runtimeId)
		{
			var result = (IRuntimeObjectView)cardViews.FirstOrDefault(cardView => cardView.RuntimeData.Id == runtimeId);

			return result ?? heroViews.FirstOrDefault(heroView => heroView.RuntimeData.Id == runtimeId);
		}

		public IEnumerable<IRuntimeObjectView> GetAllObjectViews()
		{
			return cardViews.OfType<IRuntimeObjectView>().Union(heroViews).ToArray();
		}

		public IHeroView GetHeroByUserId(string userId)
		{
			return heroViews?.FirstOrDefault(x => x.RuntimeData.OwnerUserId == userId);
		}
		public IHeroView GetHeroByUserId(int runtimeId)
		{
			return heroViews?.FirstOrDefault(x => x.RuntimeData.Id == runtimeId);
		}
		
		public string GetSelfHeroId()
		{
			return GetHeroByUserId(SelfId).RuntimeGameObject.Data.Id;
		}
		public string GetUserIdByOwner(Owner owner)
		{
			return owner == Owner.Self
				? SelfId
				: OpponentId;
		}
		public Owner GetOwnerByUserId(string userId)
		{
			return userId == SelfId
				? Owner.Self
				: Owner.Opponent;
		}

		public override void Dispose()
		{
			base.Dispose();
			NextDeckCard = null;
			cardViews.Clear();
			heroViews.Clear();
		}
	}
}