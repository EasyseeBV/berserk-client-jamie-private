using System;
using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class PerformEffectArgs
	{
		public string EffectDataId { get; set; }
		public IEffectRuntimeArg[] RuntimeArgs { get; set; } = null;
	}

	/// <summary>
	///     Generic type of effect execution.
	///     Used to perform any kind of keyword.
	/// </summary>
	public class PerformEffectCmd : Command<PerformEffectArgs>
	{
		protected override void OnExecute()
		{
			if (string.IsNullOrEmpty(ArgsModel.EffectDataId))
				throw new Exception("Empty effectConfigId passed!");

			LogicContext.EffectExecutor.CreateAndExecuteEffect(ArgsModel.EffectDataId, Executor, ArgsModel.RuntimeArgs, Targets);
		}
	}
}