using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Controllers
{
	public class RuntimeStateController : IRuntimeStateController
	{
		private readonly IGameContext gameContext;
		private readonly IGameLogicContext gameLogicContext;

		public RuntimeStateController(IGameContext gameContext, IGameLogicContext gameLogicContext)
		{
			this.gameContext = gameContext;
			this.gameLogicContext = gameLogicContext;
		}

		public void Process(IRuntimeGameCard target)
		{
			if (target == null)
				throw new NullReferenceException($"[{GetType().Name}] : Runtime card is missing");
			
			target.RuntimeData.OnStateChanged += (from, to) => OnStateChanged(target, from, to);
		}

		private void OnStateChanged(IRuntimeGameCard target, RuntimeState oldState, RuntimeState newState)
		{
			var ownerId = target.RuntimeData.OwnerUserId;
			var oppositeId = gameContext.PlayerRepository.GetOpposite(ownerId).UserId;
			var runtimeData = target.RuntimeData;
			
			if (oldState is RuntimeState.InDeck || newState is RuntimeState.InDeck)
			{
				gameLogicContext.LogicQueueController.Add(new ChangeDeckCount(ownerId, gameContext));
				gameLogicContext.LogicQueueController.Add(new ChangeDeckCount(oppositeId, gameContext));
			}
			
			if (oldState is RuntimeState.InHand || newState is RuntimeState.InHand)
			{
				gameLogicContext.LogicQueueController.Add(new ChangeHandCount(ownerId, gameContext), oppositeId);
			}

			switch (oldState.GetAccessLevel(), newState.GetAccessLevel())
			{
				// delete/create for all
				case (AccessLevel.All, AccessLevel.NoOne): Notify(); Delete(); return;
				case (AccessLevel.NoOne, AccessLevel.All): Create(); Notify(); return;
				
				// delete/create for owner
				case (AccessLevel.Self, AccessLevel.NoOne): Notify(ownerId); Delete(ownerId);  return;
				case (AccessLevel.NoOne, AccessLevel.Self): Create(ownerId); Notify(ownerId); return;
				
				// delete/create for opposite, notify all
				case (AccessLevel.All, AccessLevel.Self): Notify(); Delete(oppositeId); return;
				case (AccessLevel.Self, AccessLevel.All): Create(oppositeId); Notify(); return;
				
				// notify for all/owner
				case (AccessLevel.All, AccessLevel.All): Notify(); return;
				case (AccessLevel.Self, AccessLevel.Self): Notify(ownerId); return;
			}

			void Delete(string receiverId = null)
			{
				gameLogicContext.LogicQueueController.Add(new DeleteObject(runtimeData.Id), receiverId);
			}

			void Create(string receiverId = null)
			{
				gameLogicContext.LogicQueueController.Add(new CreateObject(runtimeData), receiverId);
			}

			void Notify(string receiverId = null)
			{
				gameLogicContext.LogicQueueController.Add(new ChangeCardsState(oldState, newState, runtimeData.Id), receiverId);
			}
		}
	}
}
