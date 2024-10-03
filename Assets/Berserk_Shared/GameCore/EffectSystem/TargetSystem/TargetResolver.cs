using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.TargetSystem
{

	public interface ITargetResolver
	{
		IRuntimeGameObject[] GetAutoTargets(
			EffectData effectData,
			IRuntimeGameObject executor,
			params IRuntimeGameObject[] manualTargets);

		void AddManualTargets(int runtimeId, params IRuntimeGameObject[] targets);
		void RemoveManualTargets(int runtimeId);
		IRuntimeGameObject[] GetManualTargets(int runtimeId);
		bool HasManualTargets(int runtimeId);
	}

	public class TargetResolver : ITargetResolver
	{
		private readonly ITargetConditionRepository targetConditionRepository;
		private readonly Dictionary<int, List<IRuntimeGameObject>> manualTargetsProvider;

		public TargetResolver(ITargetConditionRepository targetConditionRepository)
		{
			this.targetConditionRepository = targetConditionRepository;
			manualTargetsProvider = new Dictionary<int, List<IRuntimeGameObject>>();
		}

		public IRuntimeGameObject[] GetAutoTargets(
			EffectData effectData,
			IRuntimeGameObject executor,
			params IRuntimeGameObject[] manualTargets)
		{
			
			return effectData.TargetMod switch
			{
				EffectTargetMod.NeighboursLeft or EffectTargetMod.NeighboursRight or EffectTargetMod.NeighboursBoth 
					=> GetNeighbourTargets(executor, effectData, manualTargets).ToArray(),

				EffectTargetMod.Random => targetConditionRepository
					.GetAllowedTargets(executor, effectData, manualTargets)
					.TakeNRandom(effectData.GetMaxTargetCount()),

				EffectTargetMod.Self => executor == null || !targetConditionRepository.IsAllowedTarget(executor, executor, effectData, manualTargets)
					? Array.Empty<IRuntimeGameObject>()
					: new[] {executor},

				EffectTargetMod.EntityAttack or EffectTargetMod.PlayerPicked or EffectTargetMod.None => manualTargets
					.Where(target => targetConditionRepository.IsAllowedTarget(executor, target, effectData, manualTargets))
					.TakeNRandom(effectData.GetMaxTargetCount()),

				_ => throw new ArgumentOutOfRangeException($"[TargetResolver] — {effectData.TargetMod} " +
				                                           $"unsupported to auto assign targets by {executor.Data}")
			};
		}

		#region ManualTargetProvider

		public bool HasManualTargets(int runtimeId)
		{
			return manualTargetsProvider.ContainsKey(runtimeId);
		}

		public IRuntimeGameObject[] GetManualTargets(int runtimeId)
		{
			if (!HasManualTargets(runtimeId))
				return Array.Empty<IRuntimeGameObject>();

			return manualTargetsProvider[runtimeId].ToArray();
		}

		public void AddManualTargets(int runtimeId, params IRuntimeGameObject[] targets)
		{
			if (!HasManualTargets(runtimeId))
				manualTargetsProvider.Add(runtimeId, new List<IRuntimeGameObject>());

			manualTargetsProvider[runtimeId].AddRange(targets);
		}

		public void RemoveManualTargets(int runtimeId)
		{
			if (!HasManualTargets(runtimeId))
				return;

			manualTargetsProvider.Remove(runtimeId);
		}

		#endregion

		private IEnumerable<IRuntimeGameObject> GetNeighbourTargets(
			IRuntimeGameObject executor,
			EffectData effectData,
			params IRuntimeGameObject[] manualTargets)
		{
			var targets = targetConditionRepository.GetAllowedTargets(executor, effectData, manualTargets);
			if (executor is not IRuntimeGameCard fromCard)
				return targets.TakeNRandom(effectData.GetMaxTargetCount());

			var targetCards = targets
				.OfType<IRuntimeGameCard>()
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.ToArray();
			
			if (targetCards.Length == 0)
				return targetCards;
			
			var count = effectData.GetMaxTargetCount();
			var position = fromCard.RuntimeData.RelativePositionX;
			return effectData.TargetMod switch
			{
				EffectTargetMod.NeighboursLeft => GetElements(targetCards, position, Direction.Left).Take(count),
				EffectTargetMod.NeighboursRight => GetElements(targetCards, position, Direction.Right).Take(count),
				EffectTargetMod.NeighboursBoth => GetElementsBoth(targetCards, position, count),
				_ => targets
			};
		}
		
		private static IRuntimeGameCard[] GetElements(IEnumerable<IRuntimeGameCard> targets, int position, Direction direction)
		{
			return direction switch
			{
				// The left side should be reversed. e.g. positions: |0|1|2||(I'm here)|4|5|6|.
				// The left will be |0|1|2| and the right will be |4|5|6|.
				// When you try LeftCollection.Take(N), your choice will be reversed: |2|1|0|(I'm here)|4|5|6|.
				// But on the right side RightCollection.Take(N) works on linearly from index 0, it will be the same result without reverse.
				// The result in a list will look like this : |2|(I am here)|4| or |1|2|(I am here)|4|5|
				
				Direction.Left => targets.Where(x => x.RuntimeData.RelativePositionX < position).Reverse().ToArray(),
				Direction.Right => targets.Where(x => x.RuntimeData.RelativePositionX > position).ToArray(),
				_ => throw new ArgumentException("Invalid direction. Use Direction.Left or Direction.Right.")
			};
		}
		
		private IEnumerable<IRuntimeGameCard> GetElementsBoth(IRuntimeGameCard[] targets, int position, int count)
		{
			var leftSideCards = GetElements(targets, position, Direction.Left);
			var rightSideCards = GetElements(targets, position, Direction.Right);
			if (count % 2 == 0) 
				return leftSideCards.Take(count / 2).Concat(rightSideCards.Take(count / 2));
			
			var preferSideCount = (int)Math.Ceiling(count / 2d);
			var otherSideCount = count - preferSideCount;
			return leftSideCards.Length >= rightSideCards.Length 
				? leftSideCards.Take(preferSideCount).Concat(rightSideCards.Take(otherSideCount)) 
				: rightSideCards.Take(preferSideCount).Concat(leftSideCards.Take(otherSideCount));
		}
		
		private enum Direction
		{
			Left,
			Right
		}
	}
}