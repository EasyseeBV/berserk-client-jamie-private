using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialProgressRepository
	{
		Task InitAsync(IEnumerable<string> ids);

		Task SaveAsync();

		void SetCompleted(string id);

		void SetUnCompleted(string id);

		bool IsCompleted(string id);

		bool IsCompleted();

		void CompleteAll();

		void ResetAll();
	}
}