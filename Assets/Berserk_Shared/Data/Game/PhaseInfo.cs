using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.Data.Game
{

	public struct PhaseInfo
	{
		public readonly EffectPhase Phase;
		public readonly IRuntimeGameObject Initiator; // who is triggered phase change
		public readonly IRuntimeGameObject Executor;  // who will handled phase change
		public readonly IRuntimeGameObject[] Targets; // custom phase targets
		public readonly DamageType DamageType; // was a type of damage used for trigger the phase?
		
		public PhaseInfo(
			EffectPhase phase, 
			IRuntimeGameObject initiator, 
			IRuntimeGameObject executor, 
			DamageType damageType,
			params IRuntimeGameObject[] targets)
		{
			Phase = phase;
			Initiator = initiator;
			Executor = executor;
			Targets = targets;
			DamageType = damageType;
		}
	}

}