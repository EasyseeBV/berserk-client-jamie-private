using System.Threading.Tasks;
using BerserkV3.Lobby;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Applications;
using BerserkV3.Startup.Authorization;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.LobbyHandlers
{
	public class TutorialEndHandler : ITutorialCloseHandler, ITutorialOrderable, ITutorialIdentity
	{
		private readonly IGuestApplication guestApplication;
		public int Order => int.MaxValue;

		public string[] Ids { get; } =
		{
			TutorialTrigger.TutorialEnd.ToString()
		};
		
		public TutorialEndHandler(IGuestApplication guestApplication)
		{
			this.guestApplication = guestApplication;
		}

		public async Task CloseAsync(ITutorialHintEntity hint)
		{
			if (!User.IsAuthorized || !User.IsAnonymous) 
				return;
			
			await guestApplication.ExecuteActionAsync(GuestAction.TutorialEnd);
		}
	}
}