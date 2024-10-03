using System.Threading.Tasks;

namespace RR.Game.TutorialSystemV2.Abstraction.Handlers
{
	public interface ITutorialInvokeHandler : ITutorialHandler
	{
		Task InvokeAsync(ITutorialHintEntity hint);
	}
}