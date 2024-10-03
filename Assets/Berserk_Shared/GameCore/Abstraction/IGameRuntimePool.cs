using System.Collections.Generic;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IGameRuntimePool : IList<IRuntimeGameObject>
	{
		void AddRange(IEnumerable<IRuntimeGameObject> gameObjects);
		void RemoveRage(IEnumerable<IRuntimeGameObject> gameObjects);

		IRuntimeGameObject Get(int id);
		
		bool TryGet(int id, out IRuntimeGameObject result);

		IEnumerable<IRuntimeGameObject> GetMany(IEnumerable<int> ids);

		IEnumerable<IRuntimeGameObject> GetAllTableObjects(string userId = null);

		IEnumerable<IRuntimeGameObject> GetAllObjectsByUserId(string userId);

		void SetRandomIndex(IRuntimeGameObject runtimeGameObject);

		void SetFirstIndex(IRuntimeGameObject runtimeGameObject);

		void SetLastIndex(IRuntimeGameObject runtimeGameObject);

		IRuntimeHero GetHeroByUserId(string userId);
		
		IRuntimeHero GetHeroById(int runtimeId);
		IEnumerable<IRuntimeHero> GetHeroes();
		IEnumerable<IRuntimeGameCard> GetAllCards();
		IEnumerable<IRuntimeGameCard> GetAllCardsByUserId(string userId);
	}
}