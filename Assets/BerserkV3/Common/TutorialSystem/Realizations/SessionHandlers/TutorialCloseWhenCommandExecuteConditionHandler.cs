using System;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Utils;
using GameCore;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialCloseWhenCommandExecuteConditionHandler : ITutorialInvokeHandler, 
		ITutorialRemoveHandler, ITutorialCloseHandler
	{
		private const string TRIGGER_ID = "CloseWhenCommandExecute";
		private readonly IBerserkTutorialApplication tutorialApplication;
		private Action unsubscribeAction;
		
		public TutorialCloseWhenCommandExecuteConditionHandler(IBerserkTutorialApplication tutorialApplication)
		{
			this.tutorialApplication = tutorialApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (unsubscribeAction != null
			    || hint?.Conditions == null
			    || !hint.Conditions.TryGet(x => x.Id == TRIGGER_ID, out var condition)
			    || string.IsNullOrEmpty(condition.Meta))
				return Task.CompletedTask;
			
			GameCoreBus.OnCommandExecute.SubscribeRaw(HandleExecutedCommnad);
			unsubscribeAction = () => GameCoreBus.OnCommandExecute.Unsubscribe(HandleExecutedCommnad);
			
			void HandleExecutedCommnad(string cmd)
			{
				if (cmd != condition.Meta)
					return;
				
				unsubscribeAction?.Invoke();
				unsubscribeAction = null;
				tutorialApplication.CloseAsync().Forget();
			}
			
			return Task.CompletedTask;
		}
		
		public Task RemovedAsync()
		{
			if (unsubscribeAction == null)
				return Task.CompletedTask;
			
			unsubscribeAction();
			unsubscribeAction = null;
			return Task.CompletedTask;
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			if (unsubscribeAction == null)
				return Task.CompletedTask;
			
			unsubscribeAction();
			unsubscribeAction = null;
			return Task.CompletedTask;
		}
	}

}