using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.UI;
using RR.Core.DebugSystem;
namespace BerserkV3.GameCore.Cards
{
	public class HeroViewFactory : TableBaseFactory, IHeroViewFactory
	{
		private readonly IGameContainers gameContainers;
		private readonly IEnumerable<IHeroHolderView> holderViews;
		private readonly List<IHeroHolderController> holderControllers;
		protected override string ResourceName => "HeroView";
		
		public HeroViewFactory(IGameContainers gameContainers, IEnumerable<IHeroHolderView> holderViews)
		{
			this.gameContainers = gameContainers;
			this.holderViews = holderViews;
			holderControllers = new List<IHeroHolderController>();
		}
		
		public IHeroView Create(IRuntimeGameObject runtimeGameObject)
		{
			if (runtimeGameObject is not IRuntimeHero runtimeHero)
			{
				RRLogger.Error($"You trying create not a hero in hero factory!!! : {runtimeGameObject?.GetType().Name}");
				return default;
			}

			var isSelf = GameRepository.SelfId == runtimeHero.RuntimeData.OwnerUserId;
			var holderViewOwner = GameRepository.GetOwnerByUserId(runtimeHero.RuntimeData.OwnerUserId);
			var holderView = holderViews.FirstOrDefault(x => x.ViewOwner == holderViewOwner);
			var holderController = Instantiator.Instantiate<HeroHolderController>(new object[] {holderView, runtimeHero});

			var heroSpawnPoint = isSelf ? gameContainers.SelfHeroContainer : gameContainers.OpponentHeroContainer;
			var heroView = Create(runtimeHero, gameContainers.TableContainer);
			heroView.SelfContainer.position = heroSpawnPoint.position;
			EnsureHeroRenderPriority(heroView.SelfContainer);
			holderControllers.Add(holderController);
			holderController.Initialize();
			return (IHeroView) heroView;
		}

		private static void EnsureHeroRenderPriority(UnityEngine.Transform heroTransform)
		{
			if (!heroTransform)
				return;

			heroTransform.SetAsLastSibling();
		}

		public override void Dispose()
		{
			base.Dispose();
			holderControllers.ForEach(x => x?.Dispose());
			holderControllers.Clear();
		}
	}
}
