using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Startup.Abstractions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{
	public class TutorialFirstVulcaniteHandler : ITutorialInvokeHandler, ITutorialIdentity, ITutorialOrderable
	{
		private readonly ITutorialApplication tutorialApplication;
		private readonly IFirstVulcaniteApplication firstVulcaniteApplication;
		public int Order => int.MaxValue;
		public string[] Ids { get; } =
		{
			TutorialTrigger.FirstVulcanite.ToString()
		};

		public TutorialFirstVulcaniteHandler(
			ITutorialApplication tutorialApplication,
			IFirstVulcaniteApplication firstVulcaniteApplication)
		{
			this.tutorialApplication = tutorialApplication;
			this.firstVulcaniteApplication = firstVulcaniteApplication;
		}

		public async Task InvokeAsync(ITutorialHintEntity hint)
		{
			await firstVulcaniteApplication.SelectFirstVulcanteAsync();
			tutorialApplication.CloseAsync().Forget(DefaultSharedLogger.Error);
		}
	}
}