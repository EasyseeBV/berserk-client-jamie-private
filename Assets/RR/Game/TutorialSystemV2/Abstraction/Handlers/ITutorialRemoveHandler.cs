using System.Threading.Tasks;

namespace RR.Game.TutorialSystemV2.Abstraction.Handlers
{
	public interface ITutorialRemoveHandler : ITutorialHandler
	{
		Task RemovedAsync();
	}
}