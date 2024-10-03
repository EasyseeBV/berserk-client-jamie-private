using System.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using UI;

namespace BerserkV3.Common.TutorialSystem.LobbyHandlers
{

	public class TutorialDeckEditExitHandler : ITutorialCloseHandler, ITutorialOrderable, ITutorialIdentity
	{
		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.DeckEditMenuExit.ToString()
		};

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			DeckCardListView.Instance.Close();
			return Task.CompletedTask;
		}
	}
}