using System.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;

namespace BerserkV3.Common.TutorialSystem
{

	public interface IBerserkTutorialApplication : ITutorialApplication
	{
		Task InvokeAsync(object id);
		bool IsCompleted();
		bool IsCompleted(object id, bool searchPossibleKeywords = false);
	}

}