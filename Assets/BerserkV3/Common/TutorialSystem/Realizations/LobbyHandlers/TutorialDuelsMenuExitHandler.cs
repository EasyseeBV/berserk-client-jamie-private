using System.Threading.Tasks;
using BerserkV3.Lobby.MatchMaking.Duels;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.LobbyHandlers
{

	public class TutorialDuelsMenuExitHandler : ITutorialCloseHandler, ITutorialOrderable, ITutorialIdentity
	{
		private readonly IDuelsApplication duelsApplication;
		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.DuelsMenuExit.ToString()
		};
		
		public TutorialDuelsMenuExitHandler(IDuelsApplication duelsApplication)
		{
			this.duelsApplication = duelsApplication;
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			duelsApplication.Close();
			return Task.CompletedTask;
		}
	}

}