using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.DiscardHandRandom)]
	public class DiscardHandRandomVisual : EffectVisual
	{
		private readonly ICardsStateMachine cardsStateMachine;
		
		public DiscardHandRandomVisual(ICardsStateMachine cardsStateMachine)
		{
			this.cardsStateMachine = cardsStateMachine;
		}
		
		public override UniTask EndEffectAsync()
		{
			return cardsStateMachine.RearrangeHandCardsAsync(false, Owner.Self, GetSelfHandCardViews());
		}
		
		private ICardView[] GetSelfHandCardViews()
		{
			return GameRepository.CardViews
				.Where(x => x.IsSelf && x.RuntimeData.State is RuntimeState.InHand)
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.ToArray();
		}
	}
}