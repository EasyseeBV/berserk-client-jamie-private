using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.RuntimeObjects;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.Cards
{

	public class OpponentHandCardView : CardView
	{
		protected override void OnSetup()
		{
			if (!SelfContainer)
			{
				RRLogger.Error("CardView missing self container or view already destroying");
				return;
			}

			layouts.Values.SelectMany(x=> x.Values).Distinct().ForEach(x=>
			{
				x.IsSelf = IsSelf;
				x.Setup(RuntimeGameObject);
				x.Disable();
			});
			ResolveStrategy(RuntimeState.InHand);
		}

		protected override void ChangeLayoutTo(RuntimeState state)
		{
			if (!layouts.TryGetValue(RuntimeState.InHand, out var typeLayoutDict)
			    || !typeLayoutDict.TryGetValue(ObjectType.Creature, out var layout))
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Layout doesn't exist for state : {state} and type : {ObjectType.Creature}");
				return;
			}
			Layout?.Disable();
			Layout = layout;
			Layout?.Enable();
		}
	}

}