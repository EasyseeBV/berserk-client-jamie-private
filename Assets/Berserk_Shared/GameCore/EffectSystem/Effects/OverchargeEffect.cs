using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Overcharge)]
	public class OverchargeEffect : KeywordEffect
	{
		public override void Create()
		{
			base.Create();

			var availableCount = GetAvailableCount();
			var targetCount = GetTargetCount(availableCount);
			RuntimeData.AccessLevel |= AccessLevel.Self;
			RuntimeData.RuntimeArgs.Add(new OverchargeRuntimeArg
			{
				RuntimeEffectId = RuntimeData.Id,
				Count = targetCount, 
			});
			
			if (targetCount == 0)
				Expire();
		}

		protected override void OnExecute()
		{
			var arg = RuntimeData.GetRuntimeArg<OverchargeRuntimeArg>();
			if (arg == null)
				return;
			
			RuntimeData.RuntimeArgs.Remove(arg);
			SummonCards(arg.Count);
		}

		protected virtual void SummonCards(int count)
		{
			count = Math.Clamp(count, 0, GetTargetCount(GetAvailableCount()));
			SpendLava(count);

			for (var i = 0; i < count; i++)
			{
				var target = LogicContext.RuntimeFactory.CreateRuntimeCard(EffectData.Meta, Executor.RuntimeData.OwnerUserId);
				target.TurnToken(true);
				var model = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, new PlayCardArgs())
				{
					ExecutorObjectId = target.RuntimeData.Id,
				};
				LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(Executor.RuntimeData.OwnerUserId, model, true);
			}
		}

		protected override void OnExpire()
		{
			var arg = RuntimeData.GetRuntimeArg<OverchargeRuntimeArg>();
			if (arg == null)
			{
				base.OnExpire();
				return;
			}
			
			RuntimeData.RuntimeArgs.Remove(arg);
			SummonCards(arg.Count);
			base.OnExpire();
		}

		protected virtual void SpendLava(int cost)
		{
			GameContext.PlayerRepository.Get(Executor.RuntimeData.OwnerUserId)?.SpendLava(GetLavaCost(cost));
		}

		protected virtual int GetAvailableCount()
		{
			return ValueModRounded(null, EffectValue.FreeSelfTableSpace);
		}

		protected virtual int GetTargetCount(int maxAvailable)
		{
			return Math.Min(ValueModRounded(null, EffectValue.SelfLava), maxAvailable);
		}

		protected virtual int GetLavaCost(int cost)
		{
			return Math.Max(0, cost);
		}
	}
}