using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Controllers
{
	public class RuntimeCardPositionController : IRuntimeCardPositionController
	{
		private readonly IGameContext gameContext;
		private readonly IGameLogicContext gameLogicContext;

		public RuntimeCardPositionController(IGameContext gameContext, IGameLogicContext gameLogicContext)
		{
			this.gameContext = gameContext;
			this.gameLogicContext = gameLogicContext;
		}
		
		public void Process(IRuntimeGameCard target)
		{
			if (target == null)
				throw new NullReferenceException($"[{GetType().Name}] : Runtime card is missing");
			
			target.RuntimeData.OnEarlyStateChanged += (_,_) => RecalculatePositions(target);
		}
		
		public void RecalculatePositions(IRuntimeGameCard changed, bool notify = true)
		{
			if (changed == null)
				return;

			var currentState = changed.RuntimeData.State;
			var previousState = changed.RuntimeData.PreviousState;
			var cardGroups = gameContext.GameRuntimePool
				.GetCardsFilterBy(userId: changed.RuntimeData.OwnerUserId, asQuery: true)
				.Where(x => changed.RuntimeData.Id != x.RuntimeData.Id 
				            && (x.RuntimeData.State == currentState || x.RuntimeData.State == previousState))
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.GroupBy(x => x.RuntimeData.State)
				.ToArray();
			
			if (cardGroups.Length == 0 || cardGroups.All(x=> x.Key != currentState))
				Apply(changed, 0);
			
			foreach (var cardGroup in cardGroups)
			{
				if (cardGroup.Key.GetAccessLevel() == AccessLevel.NoOne)
					continue;
				
				var oredered = cardGroup.ToList();
				if (cardGroup.Key == currentState)
				{
					var currPosition = changed.RuntimeData.RelativePositionX;
					var startPosition = currPosition < 0 ? oredered.Count : currPosition;
					oredered.Insert(Math.Clamp(startPosition, 0, oredered.Count), changed);
				}
				
				var position = 0;
				oredered.ForEach(target => Apply(target, position++));
			}

			void Apply(IRuntimeGameCard target, int position)
			{
				if (target == null)
					return;
				
				target.RuntimeData.SetRelativePositionX(position);
				if (!notify || target.GetAccessLevel() == AccessLevel.NoOne)
					return;

				gameLogicContext.LogicQueueController.Add(new ChangeCardPosition(target.RuntimeData), target.GetAccessibleReceiver());
			}
		}
	}
}