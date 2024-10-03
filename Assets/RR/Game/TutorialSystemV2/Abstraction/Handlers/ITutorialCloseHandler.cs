using System.Threading.Tasks;

namespace RR.Game.TutorialSystemV2.Abstraction.Handlers
{
	public interface ITutorialCloseHandler : ITutorialHandler
	{
		Task CloseAsync(ITutorialHintEntity hint);
	}
}