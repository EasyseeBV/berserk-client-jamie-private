using System;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.Repository;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialPreviewCardHandler : ITutorialInvokeHandler, ITutorialCloseHandler, ITutorialIdentity
	{
		private readonly IPreviewView previewView;
		private readonly IPreviewSystem previewSystem;
		private readonly IGameRepository gameRepository;
		private bool isShown;

		public string[] Ids { get; } =
		{
			TutorialTrigger.PreviewCard.ToString()
		};

		public TutorialPreviewCardHandler(
			IPreviewView previewView,
			IPreviewSystem previewSystem,
			IGameRepository gameRepository)
		{
			this.previewView = previewView;
			this.previewSystem = previewSystem;
			this.gameRepository = gameRepository;
		}

		public async Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (isShown
			    || hint?.Conditions == null
			    || !hint.Conditions.TryGet(x => Ids.Contains(x.Id), out var condition)
			    || string.IsNullOrEmpty(condition.Meta))
				return;

			var args = condition.Meta.Split(',');
			if (args.Length < 2)
			{
				DefaultSharedLogger.Error(
					$"[{GetType().Name.Orange()}] Length of args less than 2, args: {args.ReflectionFormat()}");
				return;
			}

			if (!Enum.TryParse(args[0], out RuntimeState targetState))
			{
				DefaultSharedLogger.Error(
					$"[{GetType().Name.Orange()}] Unknown target state, args: {args.ReflectionFormat()}");
				return;
			}

			var targetCardId = args[1];
			if (string.IsNullOrEmpty(targetCardId))
			{
				DefaultSharedLogger.Error(
					$"[{GetType().Name.Orange()}] Unknown target cardId, args: {args.ReflectionFormat()}");
				return;
			}

			var cardView = gameRepository.CardViews.FirstOrDefault(x => x.RuntimeData.State == targetState
			                                             && x.RuntimeGameObject.Data.Id == targetCardId);

			if (cardView == null)
			{
				DefaultSharedLogger.Error(
					$"[{GetType().Name.Orange()}] Target card not found, args: {args.ReflectionFormat()}");
				return;
			}

			isShown = true;
			previewSystem.Close();
			previewSystem.Lock(true);
			await Task.Yield();
			previewView.InitAndShowCenter(cardView.RuntimeGameObject.Data.ToPreviewData(), -1);
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			if (!isShown)
				return Task.CompletedTask;

			isShown = false;
			previewSystem.Close();
			previewView.Close();
			previewSystem.Lock(false);
			return Task.CompletedTask;
		}
	}

}