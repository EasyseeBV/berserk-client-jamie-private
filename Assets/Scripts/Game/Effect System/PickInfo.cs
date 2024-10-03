using Game.Entities;
using Vulcan.Data;

namespace Game.Effect_System
{
	public class PickInfo
	{
		public IInteractiveEntity From;
		public IInteractiveEntity[] Targets;
		public EffectState EffectState;
		public bool IsFromResolver;

		public PickInfo()
		{
		}

		public PickInfo(EffectState state, IInteractiveEntity from)
		{
			EffectState = state;
			From = from;
		}
	}
}