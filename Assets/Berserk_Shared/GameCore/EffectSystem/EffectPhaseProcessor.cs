using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem
{
	public interface IEffectPhaseProcessor
	{
		void HandleRuntimeObject(IRuntimeGameObject gameObject);
	}
	
	public class EffectPhaseProcessor : IEffectPhaseProcessor
	{
		private readonly IGameContext gameContext;
		private readonly IGameLogicContext gameLogicContext;

		public EffectPhaseProcessor(IGameContext gameContext, IGameLogicContext gameLogicContext)
		{
			this.gameContext = gameContext;
			this.gameLogicContext = gameLogicContext;
		}

		public void HandleRuntimeObject(IRuntimeGameObject gameObject)
		{
			SubscribeOnPhaseChanged(gameObject);
			SubscribeStatChanges(gameObject.RuntimeData);
			SubscribeBuffEffectChanges(gameObject);
		}

		private void SubscribeStatChanges(IRuntimeData runtime)
		{
			runtime.Hp.OnChanged += _ =>
			{
				var changeObj = new ChangeObjectHp(runtime.Hp, runtime.Id);
				gameLogicContext.LogicQueueController.Add(changeObj, runtime.GetAccessibleReceiver());
			};
			
			runtime.Attack.OnChanged += _ =>
			{
				var changeObj = new ChangeObjectAttack(runtime.Attack, runtime.Id);
				gameLogicContext.LogicQueueController.Add(changeObj, runtime.GetAccessibleReceiver());
			};
			
			runtime.Armor.OnChanged += _ =>
			{
				var changeObj = new ChangeObjectArmor(runtime.Armor, runtime.Id);
				gameLogicContext.LogicQueueController.Add(changeObj, runtime.GetAccessibleReceiver());
			};
			
			runtime.Mana.OnChanged += _ =>
			{
				var changeObj = new ChangeObjectMana(runtime.Mana, runtime.Id);
				gameLogicContext.LogicQueueController.Add(changeObj, runtime.GetAccessibleReceiver());
			};
			
			runtime.MoveCount.OnChanged += _ =>
			{
				var changeObj = new ChangeObjectMoves(runtime.MoveCount, runtime.Id);
				gameLogicContext.LogicQueueController.Add(changeObj, runtime.GetAccessibleReceiver());
			};

			if (runtime.Type != ObjectType.Hero) 
				return;
			
			var runtimeHero = gameContext.GameRuntimePool.GetHeroById(runtime.Id);
			var runtimePlayer = gameContext.PlayerRepository.Get(runtime.OwnerUserId);
			
			runtimePlayer.OnLavaChanged += () => runtimeHero.ChangeEffectPhase(EffectPhase.PlayerManaChanged);
			runtimePlayer.OnDataChanged += () => gameLogicContext.LogicQueueController.Add(new ChangePlayerState(runtimePlayer.RuntimeData));
			
			runtimeHero.RuntimeData.AbilityMoveCount.OnChanged += _ =>
			{
				var changeObj = new ChangeObjectAbilityMoves(runtimeHero.RuntimeData.AbilityMoveCount, runtimeHero.RuntimeData.Id);
				gameLogicContext.LogicQueueController.Add(changeObj, runtimeHero.RuntimeData.GetAccessibleReceiver());
			};
		}

		private void SubscribeBuffEffectChanges(IRuntimeGameObject runtimeObject)
		{
			runtimeObject.OnRestore += stat =>
			{
				var objectRestoreEvent = new ObjectRestoreEvent
				{
					RuntimeArg = new ObjectStatEffectArg
					{
						RuntimeId = runtimeObject.RuntimeData.Id,
						StatName = stat.Name,
						Default = stat.BaseStat,
						From = stat.Previous,
						To = stat.Current
					}
				};
				gameLogicContext.LogicQueueController.Add(objectRestoreEvent, runtimeObject.GetAccessibleReceiver());
			};			
			
			runtimeObject.OnHit += stat =>
			{
				var objectHitEvent = new ObjectHitEvent
				{
					RuntimeArg = new ObjectStatEffectArg
					{
						RuntimeId = runtimeObject.RuntimeData.Id,
						StatName = stat.Name,
						Default = stat.BaseStat,
						From = stat.Previous,
						To = stat.Current
					}
				};
				gameLogicContext.LogicQueueController.Add(objectHitEvent, runtimeObject.GetAccessibleReceiver());
			};
			
			runtimeObject.OnDie += () =>
			{
				gameLogicContext.LogicQueueController.Add(new ObjectDestructionEvent{RuntimeId = runtimeObject.RuntimeData.Id}, runtimeObject.GetAccessibleReceiver());
				if (runtimeObject.RuntimeData.Type != ObjectType.Hero) 
					return;
				
				var param = new GameEndParams {Reason = GameEndReason.None};
				var model = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash, param);
				var loserId = runtimeObject.RuntimeData.OwnerUserId;
				gameLogicContext.CommandController.Execute<GameEndCmd>(loserId, model, true);
			};
			
			runtimeObject.OnSpawn += () =>
			{
				gameLogicContext.LogicQueueController.Add(new ObjectSpawnEvent{RuntimeId = runtimeObject.RuntimeData.Id}, runtimeObject.GetAccessibleReceiver());
			};
			
			runtimeObject.OnBuffEffectAdded += effect =>
			{
				var addObj = new AddObjectEffect(effect.RuntimeData);
				gameLogicContext.LogicQueueController.Add(addObj, effect.GetAccessibleReceiver());
			};
			
			runtimeObject.OnBuffEffectChanged += effect =>
			{
				var changeObj = new ChangeObjectEffect(effect.RuntimeData);
				gameLogicContext.LogicQueueController.Add(changeObj, effect.GetAccessibleReceiver());
			};
			
			runtimeObject.OnBuffEffectDeleted += effect =>
			{
				var delObj = new DeleteObjectEffect(effect.RuntimeData);
				gameLogicContext.LogicQueueController.Add(delObj, effect.GetAccessibleReceiver());
			};
			
			runtimeObject.OnImmuneDamageAdded += damage =>
			{
				var addDamage = new AddImmuneToDamage(damage, runtimeObject.RuntimeData.Id);
				gameLogicContext.LogicQueueController.Add(addDamage, runtimeObject.GetAccessibleReceiver());
			};
			
			runtimeObject.OnImmuneDamageDeleted += damage =>
			{
				var delDamage = new DeleteImmuneToDamage(damage, runtimeObject.RuntimeData.Id);
				gameLogicContext.LogicQueueController.Add(delDamage, runtimeObject.GetAccessibleReceiver());
			};
			
			runtimeObject.OnImmuneKeywordAdded += keyword =>
			{
				var addKeyword = new AddImmuneToKeyword(keyword, runtimeObject.RuntimeData.Id);
				gameLogicContext.LogicQueueController.Add(addKeyword, runtimeObject.GetAccessibleReceiver());
			};
			
			runtimeObject.OnImmuneKeywordDeleted += keyword =>
			{
				var delKeyword = new DeleteImmuneToKeyword(keyword, runtimeObject.RuntimeData.Id);
				gameLogicContext.LogicQueueController.Add(delKeyword, runtimeObject.GetAccessibleReceiver());
			};
			
			runtimeObject.OnImpossingEffectDeleted += effectIds =>
			{
				if (effectIds is not {Length: > 0})
					return;
			
				runtimeObject.RuntimeData.ImposingEffects.RemoveAll(effectIds.Contains);
				gameLogicContext.LogicQueueController.Add(new DeleteImposingEffects(runtimeObject.RuntimeData.Id, effectIds), runtimeObject.GetAccessibleReceiver());
			};
		}

		private void SubscribeOnPhaseChanged(IRuntimeGameObject initiator)
		{
			initiator.OnPhaseChanged += (phase, targetId, damageType) =>
			{
				if (phase == EffectPhase.None)
					return;
				
				var target = gameContext.GameRuntimePool.Get(targetId);
				gameLogicContext.EffectExecutor.ExecutePhase(new PhaseInfo(phase, initiator, initiator, damageType, target));
				
				foreach (var executor in gameContext.GameRuntimePool.ToArray())
				{
					if (executor == initiator)
						continue;
					
					gameLogicContext.EffectExecutor.ExecutePhase(new PhaseInfo(phase, initiator, executor, damageType, target));
				}
			};
		}
	}
}