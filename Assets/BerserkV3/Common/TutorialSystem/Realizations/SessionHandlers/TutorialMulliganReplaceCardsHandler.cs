using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.GameCore.Cards;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialMulliganReplaceCardsHandler : ITutorialInvokeHandler, ITutorialOrderable, 
		ITutorialCloseHandler, ITutorialIdentity
	{
		private readonly ISelectionSystem selectionSystem;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private CancellationTokenSource invokeSource;

		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.MulliganReplace.ToString()
		};

		public TutorialMulliganReplaceCardsHandler(
			ISelectionSystem selectionSystem,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.selectionSystem = selectionSystem;
			this.tutorialApplication = tutorialApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (invokeSource != null
			    || hint.Conditions == null
			    || !hint.Conditions.TryGet(x => Ids.Contains(x.Id), out var condition)
			    || string.IsNullOrEmpty(condition.Meta))
				return Task.CompletedTask;
			
			invokeSource = new CancellationTokenSource();
			
			var selectedIds = new List<int>();
			var replaceCardIds = condition.Meta.Split(',');
			selectionSystem.OnSelected += SelectionTriggered;

			async void SelectionTriggered(ISelectable selectable)
			{
				if (replaceCardIds.Length == 0 
				    || invokeSource == null 
				    || invokeSource.IsCancellationRequested)
				{
					CleanupSelectableSubscription();
					return;
				}
				
				if (selectable is not ICardStrategy strategy
				    || !replaceCardIds.Contains(strategy.View.RuntimeGameObject.Data.Id))
					return;
				
				await Task.Yield(); // wait to refresh MarkedToReplace flag
				
				if (strategy.View.MarkedAsSelected)
					selectedIds.Add(strategy.View.RuntimeData.Id);
				else
					selectedIds.Remove(strategy.View.RuntimeData.Id);

				if (replaceCardIds.Length != selectedIds.Count)
					return;

				CleanupSelectableSubscription();
				await tutorialApplication.CloseAsync();
			}

			void CleanupSelectableSubscription()
			{
				selectionSystem.OnSelected -= SelectionTriggered;
				selectionSystem.Cancel();
			}
			
			return Task.CompletedTask;
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			if (invokeSource == null)
				return Task.CompletedTask;
			
			invokeSource?.Cancel();
			invokeSource?.Dispose();
			invokeSource = null;
			
			return Task.CompletedTask;
		}
	}

}