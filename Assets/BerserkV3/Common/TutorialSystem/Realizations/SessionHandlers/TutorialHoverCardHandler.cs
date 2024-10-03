using System.Threading.Tasks;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.Cards;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialHoverCardHandler : ITutorialInvokeHandler, ITutorialOrderable, 
		ITutorialCloseHandler, ITutorialIdentity
	{
		private readonly IHoveringSystem hoveringSystem;
		private readonly IPreviewSystem previewSystem;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private bool subscription;


		public int Order => -1;
		public string[] Ids { get; } =
		{
			TutorialTrigger.HoverCards.ToString()
		};

		public TutorialHoverCardHandler(
			IHoveringSystem hoveringSystem,
			IPreviewSystem previewSystem,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.hoveringSystem = hoveringSystem;
			this.previewSystem = previewSystem;
			this.tutorialApplication = tutorialApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (subscription)
				return Task.CompletedTask;
			
			subscription = true;
			previewSystem.Lock(true);
			hoveringSystem.OnHoverEnter += OnHovering;
			return Task.CompletedTask;
		}

		private void OnHovering(IHoverable hoverable)
		{
			if (!subscription || hoverable is not ICardStrategy)
				return;
			
			subscription = false;
			hoveringSystem.OnHoverEnter -= OnHovering;
			tutorialApplication.CloseAsync().Forget();
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			if (!subscription)
				return Task.CompletedTask;
			
			subscription = false;
			hoveringSystem.OnHoverEnter -= OnHovering;
			hoveringSystem.Cancel();
			previewSystem.Lock(false);
			return Task.CompletedTask;
		}
	}

}