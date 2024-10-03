using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.Repository
{
	public interface IGameRepository
	{
		string SelfId { get; }
		string OpponentId { get; }
		bool Initialized { get; }
		void Initialize(string opponentId);
		
		IEnumerable<ICardView> CardViews { get; }
		IEnumerable<IHeroView> HeroViews { get; }
		
		ICardData NextDeckCard { get; }
		
		void DeleteObjectByRuntimeId(int runtimeId);
		void RegisterCardView(ICardView cardView);
		void RegisterHeroView(IHeroView heroView);
		void UnRegisterRuntimeView(IRuntimeObjectView view);
		void UpdateNextDeckCard(ICardData nextDeckCard);

		public ICardView GetCardViewByRuntimeId(int runtimeId);
		public IRuntimeObjectView GetObjectViewByRuntimeId(int runtimeId);
		public IEnumerable<IRuntimeObjectView> GetAllObjectViews();
		public IHeroView GetHeroByUserId(string userId);
		public IHeroView GetHeroByUserId(int runtimeId);
		
		public string GetSelfHeroId();
		public string GetUserIdByOwner(Owner owner);
		public Owner GetOwnerByUserId(string userId);
	}
}