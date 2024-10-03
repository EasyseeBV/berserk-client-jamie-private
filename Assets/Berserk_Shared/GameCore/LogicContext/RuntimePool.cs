using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class RuntimePool : List<IRuntimeGameObject>, IGameRuntimePool
	{
		private readonly IRuntimeRandomGenerator generator;
		public RuntimePool(IRuntimeRandomGenerator generator)
		{
			this.generator = generator;
		}

		public void RemoveRage(IEnumerable<IRuntimeGameObject> gameObjects)
		{
			foreach (var gameObject in gameObjects.ToArray())
			{
				Remove(gameObject);
			}
		}

		public IRuntimeGameObject Get(int id)
		{
			return this.FirstOrDefault(x => x.RuntimeData.Id == id);
		}

		public bool TryGet(int id, out IRuntimeGameObject result)
		{
			return (result = Get(id)) != null;
		}

		public IEnumerable<IRuntimeGameObject> GetMany(IEnumerable<int> ids)
		{
			return this.Where(x => ids.Contains(x.RuntimeData.Id));
		}

		public IEnumerable<IRuntimeGameObject> GetAllTableObjects(string userId = null)
		{
			var excludeUserId = string.IsNullOrEmpty(userId);
			return this
				.Where(o => excludeUserId || o.RuntimeData.OwnerUserId == userId)
				.Where(o => ObjectType.TableEntitiesMask.HasFlag(o.Data.Type))
				.Where(o => o is IRuntimeHero || (o is IRuntimeGameCard card && card.RuntimeData.State == RuntimeState.InTable))
				.ToArray();
		}

		public IRuntimeHero GetHeroByUserId(string userId)
		{
			return this.OfType<IRuntimeHero>().FirstOrDefault(x => x.RuntimeData.OwnerUserId == userId);
		}

		public IRuntimeHero GetHeroById(int runtimeId)
		{
			return (IRuntimeHero)Get(runtimeId);
		}

		public IEnumerable<IRuntimeHero> GetHeroes()
		{
			return this.OfType<IRuntimeHero>().ToArray();
		}

		public IEnumerable<IRuntimeGameObject> GetAllObjectsByUserId(string userId)
		{
			return this.Where(x => x.RuntimeData.OwnerUserId == userId).ToArray();
		}
		
		public void SetRandomIndex(IRuntimeGameObject runtimeGameObject)
		{
			Remove(runtimeGameObject);
			Insert(generator.Next(Count), runtimeGameObject);
		}

		public void SetFirstIndex(IRuntimeGameObject runtimeGameObject)
		{
			Remove(runtimeGameObject);
			Insert(0, runtimeGameObject);
		}

		public void SetLastIndex(IRuntimeGameObject runtimeGameObject)
		{
			Remove(runtimeGameObject);
			Add(runtimeGameObject);
		}
		public IEnumerable<IRuntimeGameCard> GetAllCards()
		{
			return this.OfType<IRuntimeGameCard>();
		}
		public IEnumerable<IRuntimeGameCard> GetAllCardsByUserId(string userId)
		{
			return GetAllCards()
				.Where(x => x.RuntimeData.OwnerUserId == userId);
		}
	}
}
