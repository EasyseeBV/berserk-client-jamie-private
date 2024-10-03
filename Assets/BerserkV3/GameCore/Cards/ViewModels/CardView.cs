using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using GameCore;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Serialization;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Cards
{

	public partial class CardView : RuntimeObjectView, ICardView
	{
		[Serializable]
		protected class StateLayoutDict : UnitySerializedDictionary<RuntimeState, TypeLayoutDict>
		{
		}
		
		[Serializable]
		protected class TypeLayoutDict : UnitySerializedDictionary<ObjectType, BaseCardLayout>
		{
		}
		
		[SerializeField] protected StateLayoutDict layouts = new();

		private Vector2 sizeDelta;
		private Dictionary<RuntimeState, ICardStrategy> strategyPool;
		private ICardStrategyFactory strategyFactory;
		private IGameRepository gameRepository;
		private IGameContext gameContext;
		private IGameLocks gameLocks;
		private IGameLogicEventsSource gameLogicEventsSource;
		private IDragDropSystem dragDropSystem;
		private IManualArrowSystem manualArrowSystem;
		
		public new IRuntimeGameCard RuntimeGameObject => (IRuntimeGameCard)base.RuntimeGameObject;
		public new IRuntimeCardData RuntimeData => RuntimeGameObject?.RuntimeData;
		public ICardStrategy Strategy { get; protected set; }
		public IGlowView GlowView => Layout?.GlowView;
		public TargetTransform TargetTransform { get; set; }
		public virtual IRuntimeLayout Layout { get; protected set; }
		public bool MarkedAsSelected { get; protected set; }
		public bool IsSelf => RuntimeData?.OwnerUserId == gameRepository?.SelfId;
		public bool IsLocked { get; private set;}

		[Inject]
		public void Construct(
			IGameRepository gameRepository, 
			IGameLocks gameLocks,
			IGameContext gameContext,
			ICardStrategyFactory strategyFactory,
			IGameLogicEventsSource gameLogicEventsSource, 
			IDragDropSystem dragDropSystem,
			IManualArrowSystem manualArrowSystem)
		{
			this.gameRepository = gameRepository;
			this.strategyFactory = strategyFactory;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameContext = gameContext;
			this.dragDropSystem = dragDropSystem;
			this.manualArrowSystem = manualArrowSystem;
			this.gameLocks = gameLocks;
			
			sizeDelta = SelfContainer.sizeDelta;
			strategyPool = new Dictionary<RuntimeState, ICardStrategy>();
		}

		protected override void OnSetup()
		{
			layouts.Values.SelectMany(x=> x.Values).Distinct().ForEach(x=>
			{
				x.IsSelf = IsSelf;
				x.Setup(RuntimeGameObject);
				x.Disable();
			});

			gameRepository.RegisterCardView(this);
			var token = this.GetCancellationTokenOnDestroy();
			
			gameLogicEventsSource.Subscribe<TurnGame>(Refresh, token);
			gameLogicEventsSource.Subscribe<EndGame>(_ => SetLock(true), token);
			gameLogicEventsSource.Subscribe<ChangePlayerState>(Refresh, token);
			
			gameLogicEventsSource.Subscribe<AddObjectEffect>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteImposingEffects>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteObjectEffect>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteImmuneToDamage>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteImmuneToKeyword>(Refresh, token);
			gameLogicEventsSource.Subscribe<StartEffect>(Refresh, token);
			gameLogicEventsSource.Subscribe<EndEffect>(Refresh, token);
			
			gameLogicEventsSource.Subscribe<ChangeObjectHp>(ChangeCardStat, token);
			gameLogicEventsSource.Subscribe<ChangeObjectAttack>(ChangeCardStat, token);
			gameLogicEventsSource.Subscribe<ChangeObjectMana>(ChangeCardStat, token);
			gameLogicEventsSource.Subscribe<ChangeObjectMoves>(ChangeCardStat, token);
			
			gameLocks.OnByQueueChanged += OnGameLocksChanged;
			GameCoreBus.OnLocalTurnPassed.Subscribe(this, Refresh);
			manualArrowSystem.OnArrowActivityChanged += Refresh;

			dragDropSystem.OnDragStart += _ => Refresh();
			dragDropSystem.OnDroppedIn += (_, _) => Refresh();
			dragDropSystem.OnDragCanceled += (_,_) => Refresh();
		}

		public void MarkAsSelected(bool value = true)
		{
			MarkedAsSelected = value;
			Strategy?.Refresh();
		}
		
		public void SetSize(float value)
		{
			if(!SelfContainer)
				return;
			
			SelfContainer.localScale = Vector3.one * value;
			SelfContainer.sizeDelta = sizeDelta;
		}
		
		public void SetLock(bool value)
		{
			IsLocked = value || gameContext.RuntimeData.IsEnded;
			Strategy?.Refresh();
		}
		
		public void Refresh()
		{
			Strategy?.Refresh();
		}

		public void SetLocalState(RuntimeState value)
		{
			ResolveStrategy(value);
		}
		
		private void ChangeCardStat(ChangeStatBase data)
		{
			if (data.RuntimeObjectId == RuntimeData.Id)
				Refresh();
		}

		protected virtual void ChangeLayoutTo(RuntimeState state)
		{
			if(layouts == null)
				return;

			if (!layouts.TryGetValue(state, out var typeLayoutDict))
			{
				RRLogger.Error($"Missing layouts for state: {state}");
				return;
			}

			if (!typeLayoutDict.TryGetValue(RuntimeData.Type, out var nextLayout))
			{
				RRLogger.Error($"Missing layout for {nameof(ObjectType)}: {RuntimeData.Type} in state: {state}");
				return;
			}
			
			if ((IRuntimeLayout)nextLayout == Layout)
				return;
			
			Layout?.Disable();
			Layout = nextLayout;
			Layout?.Enable();
		}

		protected void ResolveStrategy(RuntimeState state)
		{
			if(strategyPool == null)
				return;
			
			if (!strategyPool.TryGetValue(state, out var nextStrategy))
				strategyPool[state] = nextStrategy = strategyFactory.Create(state, this);

			Strategy?.Disable();
			ChangeLayoutTo(state); // ChangeLayout !!Important between disable and enable strategy
			Strategy = nextStrategy;
			Strategy?.Enable();
		}

		private void OnGameLocksChanged()
		{
			if (gameContext.Timer.RuntimeData.State > TimerState.Mulligan)
				SetLock(gameLocks.ByQueueProcessed);
		}

		private void OnDestroy()
		{
			Layout?.GlowView?.Disable();
			layouts?.Values.SelectMany(x => x.Values).Distinct().ForEach(x =>
			{
				if (x.Value()) 
					x.Disable();
			});
			
			if(manualArrowSystem != null)
				manualArrowSystem.OnArrowActivityChanged -= Refresh;
			
			if(gameLocks != null)
				gameLocks.OnByQueueChanged -= OnGameLocksChanged;
			
			strategyPool?.Values.ForEach(x => x?.Disable());
			strategyPool?.Clear();
			layouts?.Clear();
			
			Layout = null;
			layouts = null;
			strategyPool = null;
			strategyFactory = null;
			TargetTransform = default;
			gameLogicEventsSource = null;
			manualArrowSystem = null;
			MarkedAsSelected = false;
			gameRepository = null;
			gameContext = null;
			gameLocks = null;
			Strategy = null;
			GameCoreBus.OnLocalTurnPassed.Unsubscribe(this);
		}
	}
}