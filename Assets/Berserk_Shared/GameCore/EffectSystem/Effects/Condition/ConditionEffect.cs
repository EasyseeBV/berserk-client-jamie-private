using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;
using Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects;
using Berserk.Shared.GameCore.LogicContext;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition
{
	[EffectKeyword(EffectKeyword.ConditionEffect)]
	public class ConditionEffect : GiveEffectsEffect
	{
		private struct MetaData
		{
			public Condition[] Conditions;
		}
		
		private struct Condition
		{
			public string EffectIds;
			public EffectConditionType Type;
			public string Args;
		}

		protected override void OnExecute()
		{
			try
			{
				foreach (var condition in JsonConvert.DeserializeObject<MetaData>(EffectData.Meta).Conditions)
				{
					var effectCondition = LogicContext.RuntimeFactory.CreateCondition(condition.Type);
					effectCondition.Setup(this, GameContext, LogicContext);
				
					if (!effectCondition.Eval(GetArgs(condition))) 
						continue;
				
					DealEffects(condition.EffectIds, GetExecutionTargets());
					break;
				}
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error($"{GetType().Name}, {nameof(MetaData)}: {EffectData.Meta}, Exception : {e}");
				throw;
			}
		}

		private object[] GetArgs(Condition condition)
		{
			return new object[]
			{
				condition.Args,
				Targets,
				Executor
			};
		}
 	}
}