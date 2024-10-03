using System;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.GameCore.Repository;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialLockCardsConditionHandler : 
		ITutorialInvokeHandler, ITutorialCloseHandler, ITutorialOrderable
	{
		public int Order => 8;
		private readonly IGameRepository gameRepository;
		private const string AT_INVOKE_TRIGGER = "AtInvokeLockCards";
		private const string AT_CLOSE_TRIGGER = "AtCloseLockCards";

		public TutorialLockCardsConditionHandler(
			IGameRepository gameRepository)
		{
			this.gameRepository = gameRepository;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			SetLock(hint, AT_INVOKE_TRIGGER);
			return Task.CompletedTask;
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			SetLock(hint, AT_CLOSE_TRIGGER);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Args pass into condition separated by ','
		/// [bool: locked]
		/// [RuntimeState: target cards state]
		/// [string: except cardIds]
		/// </summary>
		private void SetLock(ITutorialHintEntity hint, string triggerId)
		{
			if (hint?.Conditions == null
			    || !hint.Conditions.TryGet(x => x.Id == triggerId, out var condition)
			    || string.IsNullOrEmpty(condition.Meta))
				return;

			var args = condition.Meta.Split(',');
			if (args.Length < 2)
			{
				DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] Invalid args : {args.ReflectionFormat()}");
				return;
			}
			
			if (!bool.TryParse(args[0], out var locked))
			{
				DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] Can't parse bool locked state from args : {args.ReflectionFormat()}");
				return;
			}
			
			var cardState = default(RuntimeState?);
			if (Enum.TryParse(args[1], out RuntimeState state))
				cardState = state;

			var exceptCardIds = args.Length < 3 ? Array.Empty<string>() : args.Skip(2).ToArray();
			gameRepository.CardViews
				.Where(x => (!cardState.HasValue || x.RuntimeData.State == cardState) 
				            && !exceptCardIds.Contains(x.RuntimeGameObject.Data.Id))
				.ForEach(x => x.SetLock(locked));
		}
	}

}