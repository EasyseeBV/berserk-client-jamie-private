using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.Generic.UndoSystem;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{

	public class StartupController : DisposableWithCts, IInitializable
	{
		private readonly IPreviewSystem previewSystem;
		private readonly IHoveringSystem hoveringSystem;
		private readonly IDragDropSystem dragDropSystem;
		private readonly ISelectionSystem selectionSystem;
		private readonly ICardsStateMachine cardsStateMachine;
		private readonly IRuntimeEffectModelFatory effectModelFatory;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IVisualSequenceApplication visualSequenceApplication;
		private readonly ISessionProcessor sessionProcessor;
		private readonly IProgressDrawer progressDrawer;
		private readonly ICardViewFactory cardViewFactory;
		private readonly IHeroViewFactory heroViewFactory;
		private readonly IGameRepository gameRepository;
		private readonly IInstantiator instantiator;
		private readonly IUndoSystem undoSystem;
		private readonly IGameHub gameHub;
		private IDisposable tutorialHandlers;
		private IProgress<float> startupProgress;
		private readonly ISharedConfig sharedConfig;
		private readonly Dictionary<string, int> baseManaByCardTitle = new Dictionary<string, int>();
		public StartupController(
			IPreviewSystem previewSystem,
			IHoveringSystem hoveringSystem,
			IDragDropSystem dragDropSystem,
			ISelectionSystem selectionSystem,
			ICardsStateMachine cardsStateMachine,
			IRuntimeEffectModelFatory effectModelFatory,
			IBerserkTutorialApplication tutorialApplication,
			IGameLogicEventsSource gameLogicEventsSource,
			IVisualSequenceApplication visualSequenceApplication,
			ISessionProcessor sessionProcessor,
			IProgressDrawer progressDrawer,
			ICardViewFactory cardViewFactory,
			IHeroViewFactory heroViewFactory,
			IGameRepository gameRepository,
			IInstantiator instantiator,
			IUndoSystem undoSystem,
			IGameHub gameHub,
			ISharedConfig sharedConfig)
		{
			this.previewSystem = previewSystem;
			this.hoveringSystem = hoveringSystem;
			this.dragDropSystem = dragDropSystem;
			this.selectionSystem = selectionSystem;
			this.cardsStateMachine = cardsStateMachine;
			this.effectModelFatory = effectModelFatory;
			this.tutorialApplication = tutorialApplication;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.visualSequenceApplication = visualSequenceApplication;
			this.sessionProcessor = sessionProcessor;
			this.progressDrawer = progressDrawer;
			this.cardViewFactory = cardViewFactory;
			this.heroViewFactory = heroViewFactory;
			this.gameRepository = gameRepository;
			this.instantiator = instantiator;
			this.undoSystem = undoSystem;
			this.gameHub = gameHub;
			this.sharedConfig = sharedConfig;
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<CreateObject>(_ => SpawnMissingCardViews(), Token);
			gameLogicEventsSource.Subscribe<InitializeGame>(InitializeGameAsync, Token);
			startupProgress = progressDrawer.CreateProgress();
		}

		public override void Dispose()
		{
			base.Dispose();
			undoSystem.Clear();
			selectionSystem.Clear();
			previewSystem.Clear();
			hoveringSystem.Clear();
			dragDropSystem.Clear();
			tutorialHandlers?.Dispose();
			tutorialHandlers = null;
			startupProgress?.Report(1f);
			startupProgress = null;
		}

		private async UniTask InitializeGameAsync(InitializeGame data)
		{
			try
			{
				baseManaByCardTitle.Clear();
				if (!data.ReInitialize)
					await gameHub.PerformCommandAsync<ReadyToPlayCmd>();

				if (data.RuntimeContextData.MatchMode == MatchMode.Tutorial && tutorialHandlers == null)
					tutorialHandlers = instantiator.Instantiate<TutorialSessionHandlersInstaller>();
				
				var runtimeObjects = CreateRuntimeObjects(data.GameRuntimeDatas);
				SpawnMissingHeroViews();
				InitializeViews(SpawnMissingCardViews());
				RestoreAppliedEffects(runtimeObjects);
				await tutorialApplication.InvokeAsync(TutorialTrigger.Welcome);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				gameHub.RestartRequired();
			}
			finally
			{
				startupProgress?.Report(1f);
				startupProgress = null;
			}
		}

		private IEnumerable<IRuntimeGameObject> CreateRuntimeObjects(IEnumerable<IRuntimeData> runtimeDatas)
		{
			return runtimeDatas?
				.Select(runtimeData => sessionProcessor.LogicContext.RuntimeFactory
				.CreateRuntimeObject(runtimeData, false))
				.ToList() ?? new List<IRuntimeGameObject>();
		}

		private void SpawnMissingHeroViews()
		{
			sessionProcessor.Context.GameRuntimePool.GetHeroes()
				.Where(obj => gameRepository.HeroViews.All(view => obj.RuntimeData.Id != view.RuntimeData.Id))
				.ForEach(x => heroViewFactory.Create(x));
		}

		private ICardView[] SpawnMissingCardViews()
		{
			return sessionProcessor.Context.GameRuntimePool.GetAllCards()
				.Where(obj => gameRepository.CardViews.All(view => obj.RuntimeData.Id != view.RuntimeData.Id))
				.Select(x =>
				{
					var view = cardViewFactory.Create(x);
					view.SetLocalState(x.RuntimeData.State);
					CalculateOffFactionLava(view);
					return view;
				}).ToArray();
		}

		public void CalculateOffFactionLava(ICardView view)
		{
			var offFactionConfig = sharedConfig.OffFactionLavaConfig;
			
			if (offFactionConfig == null || !offFactionConfig.Enabled)
				return;

			var maxLava = sharedConfig.PlayerMaxMana;
			
			var myHero = sessionProcessor.Context.GameRuntimePool.GetHeroByUserId(gameRepository.SelfId);
			var opponentHero = sessionProcessor.Context.GameRuntimePool.GetHeroByUserId(gameRepository.OpponentId);

			if (myHero == null || opponentHero == null)
			{
				RRLogger.Error("[CalculateOffFactionLava] Hero not found for Self or Opponent");
				return;
			}

			var myHeroQuadrant = myHero.Data.Quadrant;
			var opponentHeroQuadrant = opponentHero.Data.Quadrant;
			
			var cardQuadrant = view.RuntimeGameObject.Data.Quadrant;
			
			var cardTitle = view.RuntimeGameObject.Data.Title;
			int baseMana;

			if (!baseManaByCardTitle.TryGetValue(cardTitle, out baseMana))
			{
				baseMana = view.RuntimeGameObject.Data.Mana;
				baseManaByCardTitle[cardTitle] = baseMana;
				RRLogger.Log($"[OffFactionLava] Cache base mana {baseMana} for '{cardTitle}'");
			}
			else
			{
				RRLogger.Log($"[OffFactionLava] Use cached base mana {baseMana} for '{cardTitle}'");
			}
			var heroQuadrant = view.IsSelf ? myHeroQuadrant : opponentHeroQuadrant;
			
			var isNeutral =
				offFactionConfig.NeutralQuadrants != null &&
				offFactionConfig.NeutralQuadrants.Contains(cardQuadrant);

			int effectiveMana = baseMana;
			bool isOffFaction = false;

			if (!isNeutral && cardQuadrant != heroQuadrant)
			{
				isOffFaction = true;
				effectiveMana = baseMana + offFactionConfig.PenaltyPerCard;
			}
			
			if (effectiveMana > maxLava)
				effectiveMana = maxLava;
			
			/*if (isOffFaction)
				view.Layout.SetLavaTextColor(Color.red);
			else
				view.Layout.SetLavaTextColor(Color.white);*/
			
			if (view.IsSelf)
			{
				RRLogger.Log(
					$"[OffFactionLava][SELF] Hero={myHeroQuadrant}, Card={cardQuadrant}, Base={baseMana}, Effective={effectiveMana}, Title={cardTitle}, OffFaction={isOffFaction}");
			}
			else
			{
				RRLogger.Log(
					$"[OffFactionLava][OPP] Hero={opponentHeroQuadrant}, Card={cardQuadrant}, Base={baseMana}, Effective={effectiveMana}, Title={cardTitle}, OffFaction={isOffFaction}");
			}
			view.RuntimeData.Mana
				.SetMax(effectiveMana)
				.ResetToMax(true);
		}

		private void InitializeViews(ICardView[] cardViews)
		{
			if (cardViews.Length == 0)
				return;
			
			var handViews = cardViews.Where(x => x.RuntimeData.State == RuntimeState.InHand).ToArray();
			var tableViews = cardViews.Where(x => x.RuntimeData.State == RuntimeState.InTable).ToArray();
			var discardViews = cardViews.Where(x => x.RuntimeData.State == RuntimeState.InDiscard).ToArray();
			var showedAllViews = cardViews.Where(x => x.RuntimeData.State == RuntimeState.InShowAll).ToArray();

			if (handViews.Length > 0)
			{
				cardsStateMachine.SetupCardsInHand(handViews);
				cardsStateMachine.RearrangeHandCardsAsync(true, Owner.Self, handViews).Forget();
			}
			
			if (tableViews.Length > 0)
			{
				cardsStateMachine.SetupCardsOnTable(tableViews);
				cardsStateMachine.RearrangeTableAsync(true, Owner.Self).Forget();
				cardsStateMachine.RearrangeTableAsync(true, Owner.Opponent).Forget();
			}
			
			if (discardViews.Length > 0)
			{
				cardsStateMachine.SetupCardsInDiscard(discardViews);
			}

			if (showedAllViews.Length > 0)
			{
				cardsStateMachine.SetupCardsInShowAll(showedAllViews);
				cardsStateMachine.RearrangeInShowAllAsync(showedAllViews).Forget();
			}
		}

		private void RestoreAppliedEffects(IEnumerable<IRuntimeGameObject> restoreObjects)
		{
			try
			{
				if (restoreObjects == null)
					return;
				
				var restoredEffectVisuals = new List<IRuntimeEffectModel>();
				foreach (var gameObject in restoreObjects)
				{
					if (gameObject == null)
					{
						DefaultSharedLogger.Error($"{nameof(RestoreAppliedEffects)} can't restore effects for {nameof(IRuntimeGameObject)}, is null.");
						continue;
					}
					
					if (gameObject.RuntimeData == null)
					{
						DefaultSharedLogger.Error($"{nameof(RestoreAppliedEffects)} can't restore effects for {nameof(IRuntimeGameObject)}, the {nameof(IRuntimeCardData)} is null.");
						continue;
					}

					if (gameObject.AppliedEffects.Any(x => x?.RuntimeData == null))
					{
						var cleanup = gameObject.AppliedEffects.Where(x => x?.RuntimeData != null).ToArray();
						gameObject.AppliedEffects.Clear();
						gameObject.AppliedEffects.AddRange(cleanup);
						DefaultSharedLogger.Error($"{nameof(RestoreAppliedEffects)} while restore effects for {nameof(IRuntimeGameObject)}, runtimeData: {gameObject.RuntimeData.ReflectionFormat()}, applied effects has null.");
					}

					if (gameObject.RuntimeData.AppliedEffects.Any(x => x == null))
					{
						var cleanup = gameObject.RuntimeData.AppliedEffects.Where(x => x != null).ToArray();
						gameObject.RuntimeData.AppliedEffects.Clear();
						gameObject.RuntimeData.AppliedEffects.AddRange(cleanup);
						DefaultSharedLogger.Error($"{nameof(RestoreAppliedEffects)} while restore effects for {nameof(IRuntimeGameObject)}, runtimeData: {gameObject.RuntimeData.ReflectionFormat()}, applied effects runtime data is null.");
					}
					
					foreach (var runtimeEffectData in gameObject.RuntimeData.AppliedEffects)
					{
						try
						{
							if (gameObject.AppliedEffects.Any(o => o.RuntimeData.Id == runtimeEffectData.Id))
								continue;
							
							var effect = sessionProcessor.LogicContext.RuntimeFactory.CreateRuntimeEffect(runtimeEffectData);
							if (effect == null)
							{
								DefaultSharedLogger.Error($"{nameof(RestoreAppliedEffects)} created {nameof(IRuntimeEffect)} is null, runtimeData: {runtimeEffectData.ReflectionFormat()}");
								continue;
							}
							
							gameObject.AppliedEffects.Add(effect);
							var model = effectModelFatory.Create(runtimeEffectData);
							if (model == null)
							{
								DefaultSharedLogger.Error($"{nameof(RestoreAppliedEffects)} created {nameof(IRuntimeEffectModel)} is null, runtimeData: {runtimeEffectData.ReflectionFormat()}");
								continue;
							}

							restoredEffectVisuals.Add(model);
							visualSequenceApplication.InitLongEffect(model);
						}
						catch (Exception e)
						{
							DefaultSharedLogger.Error(e);
						}
					}
				}

				foreach (var effectModel in restoredEffectVisuals.OrderBy(x => (int)x.Data.VisualKeyword))
				{
					visualSequenceApplication.ApplyLongEffectAsync(effectModel, true).Forget(DefaultSharedLogger.Error);
				}
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}
	}

}