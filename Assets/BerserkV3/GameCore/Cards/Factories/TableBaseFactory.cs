using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Cards
{
	public abstract class TableBaseFactory : DisposableWithCts, IInitializable
	{
		protected abstract string ResourceName {get;}
		protected IInstantiator Instantiator;
		protected IGameRepository GameRepository;
		protected Object Prefab;
		
		[Inject]
		private void Construct(IInstantiator instantiator,
		                      IGameRepository gameRepository)
		{
			Instantiator = instantiator;
			GameRepository = gameRepository;
		}

		void IInitializable.Initialize()
		{
			Prefab = Resources.Load(ResourceName);
		}
		
		protected IRuntimeObjectView Create(IRuntimeGameObject runtimeGameObject, Transform container = null)
		{
			// Do not use Instantiator.InstantiatePrefabResourceForComponent<>(),
			// prefabs can have child objects witch same components - zenject throw Asert
			
			var view = Instantiator.InstantiatePrefab(Prefab, container).GetComponent<IRuntimeObjectView>();
			view.Setup(runtimeGameObject);
			return view;
		}
	}
}