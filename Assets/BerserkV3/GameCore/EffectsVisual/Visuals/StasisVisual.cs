using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.LogicEventsProcessor;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Stasis)]
	public class StasisVisual : FaceIdTauntingVisual
	{
		private readonly IVisualEffectsRepository visualEffectsRepository;
		
		private readonly Dictionary<int, (bool hpVisible, bool attackVisible, bool armorVisible)> localStates = new();
		
		public StasisVisual(IVisualEffectsRepository visualEffectsRepository,
			IGameLogicEventsSource gameLogicEventsSource, IGameDatabase gameDatabase) : base(gameLogicEventsSource, gameDatabase)
		{
			this.visualEffectsRepository = visualEffectsRepository;
		}
		
		public override async UniTask ApplyLongEffectAsync()
		{
			var args = Model.GetRuntimeArgs<StasisEffectArg>().ToArray();
			foreach (var target in args.Select(x => GameRepository.GetObjectViewByRuntimeId(x.EffectOwnerId)))
			{
				var layout = target switch
				{
					ICardView cardView => (ICreatureLayout)cardView.Layout,
					IHeroView heroView => (ICreatureLayout)heroView.Layout,
					_ => null
				};
				
				if (layout == null)
					continue;
				
				layout.IsAllowedExternal = true;
				localStates.TryAdd(target.RuntimeData.Id,
					(layout.IsHealthVisible, layout.IsAttackVisible, layout.IsArmorVisible));
				layout.SetActiveArmor(false);
				layout.SetActiveAttack(false);
				layout.SetActiveHealth(false);
			}
			
			await UniTask.WhenAll(visualEffectsRepository
				.GetMany(args.Select(x => x.EffectId))
				.OrderBy(x => (int)x.EffectData.VisualKeyword)
				.Select(visual => visual.ExpireLongEffectAsync())
				.Append(base.ApplyLongEffectAsync()));
		}
		
		public override async UniTask ExpireLongEffectAsync()
		{
			var args = Model.GetRuntimeArgs<StasisEffectArg>().ToArray();
			foreach (var target in args.Select(x => GameRepository.GetObjectViewByRuntimeId(x.EffectOwnerId)))
			{
				var layout = target switch
				{
					ICardView cardView => (ICreatureLayout)cardView.Layout,
					IHeroView heroView => (ICreatureLayout)heroView.Layout,
					_ => null
				};
				
				if (layout == null)
					continue;
				
				layout.IsAllowedExternal = false;
				var runtimeData = target.RuntimeData;
				if (!localStates.TryGetValue(runtimeData.Id, out var localState))
					continue;
				
				layout.SetActiveArmor(localState.armorVisible);
				layout.SetActiveHealth(localState.hpVisible);
				layout.SetActiveAttack(localState.attackVisible);
				layout.Refresh();
				localStates.Remove(runtimeData.Id);
			}
			
			await UniTask.WhenAll(visualEffectsRepository
				.GetMany(args.Select(x => x.EffectId))
				.OrderBy(x => (int)x.EffectData.VisualKeyword)
				.Select(visual => visual.ApplyLongEffectAsync())
				.Append(base.ExpireLongEffectAsync()));
		}
	}
}