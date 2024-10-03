using System;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Models;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Network.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialSessionEndHandler : ITutorialInvokeHandler, ITutorialOrderable,
		ITutorialCloseHandler, ITutorialIdentity
	{
		private static UITutorialPopup Window => UITutorialPopup.Instance;
		private readonly IGameHub gameHub;
		private readonly IRuntimeTimer runtimeTimer;
		private readonly ISceneService sceneService;
		private readonly ITutorialTextFormatter textFormatter;
		private readonly IBerserkTutorialApplication tutorialApplication;
		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.SessionEnd.ToString()
		};

		public TutorialSessionEndHandler(
			IGameHub gameHub,
			IRuntimeTimer runtimeTimer,
			ISceneService sceneService,
			ITutorialTextFormatter textFormatter,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.gameHub = gameHub;
			this.runtimeTimer = runtimeTimer;
			this.sceneService = sceneService;
			this.textFormatter = textFormatter;
			this.tutorialApplication = tutorialApplication;
		}

		public async Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (hint.Popup.Enabled)
				return;

			if (hint.Popup.DelayBeforShow > 0)
				await Task.Delay(TimeSpan.FromSeconds(hint.Popup.DelayBeforShow));
			
			Window.SetBackHinder(hint.Popup.BackHinder);
			Window.SetBackButton(hint.Popup.BackButton, CloseHint);
			Window.SetHeaderText(textFormatter.Format(hint.Popup.Title));
			Window.SetBodyText(textFormatter.Format(hint.Popup.Text));
			Window.SetAcceptButton(true, "Continue", CloseHint);
			Window.SetDeclineButton(true, "Lobby", SendEndGame);
			Window.Container.anchoredPosition = hint.Popup.Position;
			Window.Enable(true);
		}

		private void SendEndGame()
		{
			try
			{
				Window.Enable(false);
				var param = new GameEndParams {Reason = GameEndReason.None};
				var model = new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, param);
				gameHub.PerformCommandAsync<TutorialEnd>(model, true).AddLoadingTask();
				UITutorialBlockRaycasts.Instance.Enable(true); // block input under endGame is coming.
				CloseHint();
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				CloseHint();
				sceneService.LoadAsync(Scene.Lobby).AddLoadingTask();
			}
		}

		private void CloseHint()
		{
			tutorialApplication.CloseAsync().Forget();
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			if (hint.Popup.Enabled)
				return Task.CompletedTask;

			Window.Enable(false);
			return Task.CompletedTask;
		}
	}

}