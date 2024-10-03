using System.Threading.Tasks;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Lobby.MatchMaking.Practice;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{
	public class TutorialRestartHandler : ITutorialInvokeHandler, ITutorialOrderable, ITutorialIdentity
	{
		private readonly IPracticeApplication practiceApplication;

		public int Order => int.MaxValue;

		public string[] Ids { get; } =
		{
			TutorialTrigger.StartSession.ToString()
		};

		public TutorialRestartHandler(IPracticeApplication practiceApplication)
		{
			this.practiceApplication = practiceApplication;
		}

		public async Task InvokeAsync(ITutorialHintEntity hint)
		{
			await practiceApplication.StartTutorialMatchAsync().AddLoadingTask();
		}
	}
}