using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.Repository;
using BerserkV3.Generic.UndoSystem;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.GameCore
{
	public interface IVisibleLocalState
	{
		void ApplyEvent(ILogicEvent logicEvent);
	}

	public class VisibleLocalState : IVisibleLocalState
	{
		private readonly ISessionProcessor sessionProcessor;
		private readonly IGameRepository gameRepository;
		private readonly IPreviewView previewView;
		private readonly IUndoSystem undoSystem;
		private IGameRuntimePool GameRuntimePool => sessionProcessor.Context.GameRuntimePool;
		private IRuntimeFactory RuntimeFactory => sessionProcessor.LogicContext.RuntimeFactory;
		private IGameContext GameContext => sessionProcessor.Context;
		
		public VisibleLocalState(
			IGameRepository gameRepository,
			ISessionProcessor sessionProcessor,
			IPreviewView previewView,
			IUndoSystem undoSystem)
		{

			this.sessionProcessor = sessionProcessor;
			this.gameRepository = gameRepository;
			this.previewView = previewView;
			this.undoSystem = undoSystem;
		}

		public void ApplyEvent(ILogicEvent logicEvent)
		{
			switch (logicEvent)
			{
				case InitializeGame data:
					sessionProcessor.Build(data);
					break;
				
				case CommandApprove approve :
					undoSystem.Remove(approve.CommandId);
					DefaultSharedLogger.Log($"Approved command with Id : {approve.CommandId.Green()}");
					break;
				
				case CommandCancel cancel :
					undoSystem.Undo(cancel.CommandId);
					DefaultSharedLogger.Log($"Canceled command with Id : {cancel.CommandId.Red()}, Message : {cancel.Message}");
					break;
				
				case ChangePlayerState playerState:
					GameContext.PlayerRepository.Sync(playerState.RuntimePlayerData);
					break;

				case TurnGame turnGame:
					GameContext.Timer.Sync(turnGame.RuntimeData);
					break;

				case ChangedTimer changedTimer:
					GameContext.Timer.Sync(changedTimer.RuntimeData);
					break;

				case CreateObject createObject:
					var obj = RuntimeFactory.CreateRuntimeObject(createObject.RuntimeData, false);
					if (obj != null && obj.RuntimeData.OwnerUserId != gameRepository.OpponentId)
						break;
					
					if (obj is IRuntimeGameCard gameCard)
						previewView.InitAndShowCorner(gameCard.ToPreviewData(), 1);
					break;

				case DeleteObject deleteObject:
					gameRepository.DeleteObjectByRuntimeId(deleteObject.RuntimeObjectId);
					if (GameRuntimePool.TryGet(deleteObject.RuntimeObjectId, out var gameObject))
					{
						GameRuntimePool.Remove(gameObject);
						gameObject.Dispose();
					}
					break;

				case ChangeCardsState changeCardsState:
					GameRuntimePool.GetCardsFilterBy(runtimeIds: changeCardsState.CardIds.ToArray())
						.ForEach(c => c.UpdateState(changeCardsState.NewState));
					
					break;
				case ChangeCardPosition changeCardsPosition:
					GameRuntimePool.GetCardsFilterBy(runtimeIds : changeCardsPosition.Id)
						.ForEach(c => c.RuntimeData.SetRelativePositionX(changeCardsPosition.Position));
					break;

				case ChangeStatBase changedObjectStat:
					if (GameRuntimePool.TryGet(changedObjectStat.RuntimeObjectId, out var changedObject))
					{
						var stat = changedObject.RuntimeData.GetStatByName(changedObjectStat.Stat.Name);
						stat.Replace(changedObjectStat.Stat, true);
					}
					else
					{
						DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] " +
						                          $"While apply a stat : {changedObjectStat.Stat?.ToString(true)}, " +
						                          $"Target with id : {changedObjectStat.RuntimeObjectId} not found.");
					}
					break;

				case ChangeObjectEffect changeObjectEffect:
					if (!GameRuntimePool.TryGet(changeObjectEffect.RuntimeData.ExecutorId, out var objectWithEffect))
					{
						DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] " +
						                          $"While apply an effect : {changeObjectEffect.RuntimeData.Id}, " +
						                          $"Effect owner with id : {changeObjectEffect.RuntimeData.ExecutorId} not found.");
						break;
					}
					
					if (!objectWithEffect.TryGetAppliedEffect(changeObjectEffect.RuntimeData.Id, out var changedEffect))
					{
						DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] " +
						                          $"While apply an effect : {changeObjectEffect.RuntimeData.Id}, " +
						                          $"Target effect with id : {changeObjectEffect.RuntimeData.Id} not found.");
						break;
					}

					changedEffect.Sync(changeObjectEffect.RuntimeData);
					objectWithEffect.ChangedAppliedEffect(changedEffect);
					break;
				
				case AddObjectEffect addObjectEffect:
				{
					if (!GameRuntimePool.TryGet(addObjectEffect.RuntimeData.ExecutorId, out var newEffectOwner))
						return;

					var runtimeEffect = RuntimeFactory.CreateRuntimeEffect(addObjectEffect.RuntimeData);
					newEffectOwner.ApplyAppliedEffect(runtimeEffect);
				}
					break;
					
				case SetImposingEffects setImposingEffects:
				{
					if (!GameRuntimePool.TryGet(setImposingEffects.RuntimeObjectId, out var target))
						return;
					
					target.RuntimeData.ImposingEffects = new List<string>(setImposingEffects.ImposingEffectIds);
				} break;

				case DeleteObjectEffect deleteObjectEffect:
				{
					if (!GameRuntimePool.TryGet(deleteObjectEffect.RuntimeData.ExecutorId, out var target))
						return;
					
					target.RemoveAppliedEffect(deleteObjectEffect.RuntimeData.Id);
				} break;

				case DeleteImposingEffects deleteImposingEffects:
				{
					if (!GameRuntimePool.TryGet(deleteImposingEffects.RuntimeObjectId, out var target))
						return;
					
					target.RuntimeData.ImposingEffects.RemoveAll(effectId => deleteImposingEffects.ImposingEffectIds.Contains(effectId));
				} break;

				case DeleteImmuneToDamage deleteImmuneToDamage:
				{
					if (!GameRuntimePool.TryGet(deleteImmuneToDamage.RuntimeId, out var target))
						return;
					
					var removeDamages = target.RuntimeData.ImmuneToDamage
						.Where(o => o.DamageType == deleteImmuneToDamage.DamageType && o.EffectId == deleteImmuneToDamage.EffectId)
						.ToArray();

					foreach (var damage in removeDamages)
					{
						target.RemoveImmuneDamage(damage);
					}
				} break;

				case AddImmuneToDamage addImmuneToDamage:
				{
					if (!GameRuntimePool.TryGet(addImmuneToDamage.RuntimeId, out var target))
						return;
					
					var type = Type.GetType(addImmuneToDamage.TypeName);
					if (type == null)
					{
						RRLogger.Error($"[{GetType().Name.Orange()}] Unknown type of {nameof(DamageValue)} : {addImmuneToDamage.TypeName}");
						break;
					}

					var args = new object[] {addImmuneToDamage.DamageType, addImmuneToDamage.ValueMod, addImmuneToDamage.EffectId, addImmuneToDamage.Value};
					var damageValue = (DamageValue) Activator.CreateInstance(type, args);
					target.ApplyImmuneDamage(damageValue);
				} break;

				case DeleteImmuneToKeyword deleteImmuneToKeyword:
				{
					if (!GameRuntimePool.TryGet(deleteImmuneToKeyword.RuntimeId, out var target))
						return;
					
					var removeKeywords = target.RuntimeData.ImmuneToKeywords
						.Where(o => o.Keyword == deleteImmuneToKeyword.Keyword && o.EffectId == deleteImmuneToKeyword.EffectId)
						.ToArray();
					
					foreach (var keyword in removeKeywords)
					{
						target.RemoveImmuneKeyword(keyword);
					}
				} break;
				
				case AddImmuneToKeyword addImmuneToKeyword:
				{
					if (!GameRuntimePool.TryGet(addImmuneToKeyword.RuntimeId, out var target))
						return;
					target.ApplyImmuneKeyword(new ImmuneKeyword(addImmuneToKeyword.Keyword, addImmuneToKeyword.EffectId));
				} break;

				case ChangedNextDeckCard nextDeckCardChanged:
					if (nextDeckCardChanged.CardId == null)
					{
						gameRepository.UpdateNextDeckCard(null);

						break;
					}

					var cardConfig = GameContext.GameDatabase.GetCard(nextDeckCardChanged.CardId);

					if (cardConfig == null)
					{
						DefaultSharedLogger.Error($"{nameof(cardConfig)} not found by cardId {nextDeckCardChanged.CardId}");
						gameRepository.UpdateNextDeckCard(null);

						break;
					}

					gameRepository.UpdateNextDeckCard(cardConfig);

					break;
			}
		}
	}
}