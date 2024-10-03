using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;

namespace Berserk.Shared.GameCore.Commands.Cmd
{

	public class PerformPhaseArgs
	{
		public EffectPhase Phase { get; set; }
		public DamageType DamageType { get; set; }
		public int? Initiator { get; set; }
	}

	public class PerformPhaseCmd : Command<PerformPhaseArgs>
	{
		protected override void OnExecute()
		{
			if (ArgsModel.Phase == EffectPhase.None)
				return;

			var initiator = ArgsModel.Initiator.HasValue 
				? GameContext.GameRuntimePool.Get(ArgsModel.Initiator.Value)
				: Executor;

			var phaseInfo = new PhaseInfo(ArgsModel.Phase, initiator, Executor, ArgsModel.DamageType, Targets);
			
			LogicContext.EffectExecutor.ExecutePhase(phaseInfo);
		}
	}

}