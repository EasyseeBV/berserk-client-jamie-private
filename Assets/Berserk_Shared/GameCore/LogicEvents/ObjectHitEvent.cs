using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ObjectHitEvent : LogicEvent
	{
		public ObjectStatEffectArg RuntimeArg { get; set; }
	}
}