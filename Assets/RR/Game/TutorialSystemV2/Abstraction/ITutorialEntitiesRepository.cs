using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialEntitiesRepository
	{
		Task InitAsync();
		
		void ResetAll();

		ITutorialHintEntity Get(string id);
		
		IEnumerable<ITutorialHintEntity> GetAll();
	}
}