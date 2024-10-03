using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.TargetSystem;
using BerserkV3.Generic.UndoSystem;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.Cards
{

	public class CardCreatureStrategy : CardInTableStartegy
	{
		private readonly IUndoSystem undoSystem;
		private readonly IGameHub gameHub;
		private readonly IGameLogicContext gameLogicContext;

		public CardCreatureStrategy(
			IUndoSystem undoSystem,
			IGameHub gameHub,
			IGameLogicContext gameLogicContext)
		{
			this.undoSystem = undoSystem;
			this.gameHub = gameHub;
			this.gameLogicContext = gameLogicContext;
		}

		protected override void OnEnabled()
		{
			if (View?.Layout?.GlowView == null || GameContext == null)
				return;

			base.OnEnabled();
			DragDropSystem.InputHandler.OnStart += OnInputStartDragDetected;
		}

		protected override void OnDisabled()
		{
			base.OnDisabled();
			DragDropSystem.InputHandler.OnStart -= OnInputStartDragDetected;
		}

		protected override void OnRefreshed()
		{
			View?.GlowView?.Enable(IsCanAttack(), GlowType.Turn);
			base.OnRefreshed();
		}

		private bool IsCanAttack()
		{
			return View != null
			       && !ManualArrowSystem.IsActive
			       && gameLogicContext.ExecutorConditionRepository.IsCanAttack(View.RuntimeGameObject);
		}

	#region Handle Input
		private async void OnInputStartDragDetected()
		{
			if (!IsCanAttack() || !View.IsSelf)
				return;
			
			if (!InputHelper.IsPointerOver(View?.SelfContainer.gameObject))
				return;

			try
			{
				var moveCount = (int)View.RuntimeData.MoveCount;
				var manualEffect = GameContext.GameDatabase.GetEffectConfig(EffectKeyword.GenericAttack.AsSystemEffectId());
				var pickInfo = new PickInfo(View, manualEffect.Id, manualEffect.MaxTargetCount);
				var targets = await ManualArrowSystem.GetTargetsAsync(pickInfo);

				var model = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash)
				{
					ExecutorObjectId = View.RuntimeData.Id,
					TargetObjectsIds = targets.Select(runtimeView => runtimeView.RuntimeData.Id).ToList(),
				};
				
				View.RuntimeData.MoveCount.Set(moveCount - 1); // just predict
				undoSystem.Add(model.CommandId, () => View.RuntimeData.MoveCount.Set(moveCount));
				await gameHub.PerformCommandAsync<PerformAttackCmd>(model, true);
			}
			catch (OperationCanceledException)
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] Manual arrow cancelled : {View.RuntimeGameObject.Data.Title.Bold()}");
			}
		}
	#endregion
	}
}