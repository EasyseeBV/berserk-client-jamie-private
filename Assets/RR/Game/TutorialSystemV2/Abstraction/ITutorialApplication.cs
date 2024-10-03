using System.Threading.Tasks;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialApplication
	{
		Task ResetAsync();
		
		Task InitAsync();

		Task InvokeAsync(string id);

		Task CloseAsync();

		Task RestartAsync();

		Task SkipAsync();

		ITutorialHintEntity[] GetActive();
	}
}