using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using BerserkV3.Common.AudioSystem;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Mulligan)]
	public class MulliganVisual : EffectVisual
	{
		private const float REPLACE_DURATION = 0.4f;
		private CancellationTokenSource subscription;
		
		private readonly IGameHub gameHub;
		private readonly IGameContext gameContext;
		private readonly IGameRepository gameRepository;
		private readonly IGameContainers gameContainers;
		private readonly IMulliganView mulliganView;
		private readonly IAudioApplication audioApplication;

		public MulliganVisual(
			IGameHub gameHub,
			IGameContext gameContext,
			IGameRepository gameRepository,
			IGameContainers gameContainers,
			IMulliganView mulliganView,
			IAudioApplication audioApplication,
			IGameLogicEventsSource gameLogicEventsSource)
		{
			this.gameHub = gameHub;
			this.gameContext = gameContext;
			this.gameRepository = gameRepository;
			this.gameContainers = gameContainers;
			this.mulliganView = mulliganView;
			this.audioApplication = audioApplication;
			subscription = new CancellationTokenSource();
			gameLogicEventsSource.Subscribe<ChangedTimer>(TryDisplayWaitOpponent, subscription.Token);
		}

		public override void Dispose()
		{
			base.Dispose();
			subscription?.Cancel();
			subscription?.Dispose();
			subscription = null;
		}

		public override async UniTask ApplyLongEffectAsync()
		{
			var runtimePlayerData = gameContext.PlayerRepository.Get(Executor.RuntimeData.OwnerUserId).RuntimeData;
			await mulliganView
				.SetWaitingOpponentLabelVisible(false)
				.SetConfirmButtonVisible(false)
				.SetCardsContainerInteractable(false)
				.InitAndShowAsync(runtimePlayerData.IsFirstMover, () => gameContext.Timer.GetTimeLeft());

			mulliganView
				.SetWaitingOpponentLabelVisible(runtimePlayerData.IsFinishedMulligan)
				.SetConfirmButtonVisible(!runtimePlayerData.IsFinishedMulligan)
				.SetCardsContainerInteractable(!runtimePlayerData.IsFinishedMulligan);

			if (!runtimePlayerData.IsFinishedMulligan)
				mulliganView.OnAccepted += () => OnAcceptMulligan().Forget(DefaultSharedLogger.Error);

			await UniTask.WhenAll(SimulateCardShufflingAuidoAsync(), ToChooseAsync(GetTargets()));
		}

		public override async UniTask StartEffectAsync()
		{
			await ToDeckAsync(GetTargets());
		}

		public override async UniTask EndEffectAsync()
		{
			await ToChooseAsync(GetTargets());
			TryDisplayWaitOpponent();
		}

		public override async UniTask ExpireLongEffectAsync()
		{
			gameRepository.CardViews.ForEach(x => x.MarkAsSelected(false));
			await mulliganView.CloseAsync();
			await UniTask.Delay(500); // fake visual delay
		}

		private async UniTask ToChooseAsync(ICardView[] toChoose)
		{
			var existChoose = GetExistChooseCards(toChoose);
			var totalChoose = GetTotalChooseCards(existChoose, toChoose);

			SetupCardsInChoose(toChoose, true);
			SetupCardsInChoose(existChoose, false);
			RearrangeHorizontal(totalChoose);

			await AnimateCardsAsync(toChoose, 0);
		}

		private async UniTask ToDeckAsync(ICardView[] toDeck)
		{
			var existChoose = GetExistChooseCards(toDeck);
			var totalChoose = GetTotalChooseCards(existChoose, toDeck);

			SetupCardsInChoose(totalChoose, false);
			RearrangeHorizontal(totalChoose);

			await AnimateCardsAsync(toDeck, OutOfScreenPosition());
			toDeck.ForEach(x=> x.MarkAsSelected(false));
		}

		protected ICardView[] GetTargets()
		{
			return Targets
				.OfType<ICardView>()
				.OrderBy(x => x.RuntimeGameObject.RuntimeData.RelativePositionX)
				.ToArray();
		}

		private ICardView[] GetTotalChooseCards(IEnumerable<ICardView> exist, IEnumerable<ICardView> replace)
		{
			return exist.Union(replace)
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.ToArray();
		}

		private ICardView[] GetExistChooseCards(IEnumerable<ICardView> except)
		{
			return gameRepository.CardViews
				.Where(x => x.IsSelf && x.RuntimeData.State == RuntimeState.InChoose)
				.Except(except)
				.ToArray();
		}

		private void SetupCardsInChoose(IEnumerable<ICardView> newCardsInChooseViews, bool fromReplace)
		{
			newCardsInChooseViews?.ForEach(x =>
			{
				x.SelfContainer.SetParent(gameContainers.MulliganContainer, false);
				x.SelfContainer.localRotation = Quaternion.identity;
				x.SelfContainer.localPosition = fromReplace
					? OutOfScreenPosition(x.SelfContainer.localPosition)
					: Vector3.zero;
			});
		}

		private Vector3 OutOfScreenPosition(Vector3 original)
		{
			original.y = OutOfScreenPosition();
			return original;
		}

		private float OutOfScreenPosition()
		{
			return gameContainers.MulliganContainer
				.InverseTransformPoint(gameContainers.ReplaceMulliganContainer.position).y;
		}

		private static void RearrangeHorizontal(IReadOnlyList<ICardView> cardViews)
		{
			if (cardViews == null)
				return;

			var space = 150f;
			var cardSize = cardViews.Max(x => x.SelfContainer.rect.width);
			var halfCardSize = cardSize / 2f;
			var layoutOffset = (GetPosition(cardViews.Count - 1) - cardSize) / 2f;

			for (var i = 0; i < cardViews.Count; i++)
			{
				var view = cardViews[i];
				var position = view.SelfContainer.localPosition;

				position.x = GetPosition(i) - layoutOffset;
				view.SelfContainer.localPosition = position;
			}

			return;

			float GetPosition(int index)
			{
				return ((cardSize + space) * index) - (halfCardSize + space);
			}
		}

		private static async UniTask AnimateCardsAsync(IReadOnlyList<ICardView> targets, float position)
		{
			foreach (var view in targets)
				view.SetLocalState(RuntimeState.InChoose); // all cards setup inChoose state

			for (var i = 0; i < targets.Count; i++)
			{
				var cardView = targets[i];
				var isLast = i == targets.Count - 1;

				await AnimateTo(position, cardView.SelfContainer, isLast);
				cardView.SetLocalState(cardView.RuntimeData.State); // restore to normal state
			}
		}

		private static UniTask AnimateTo(float toPosition, Transform target, bool isLast)
		{
			if (!target)
				return UniTask.CompletedTask;

			target.DOKill();
			var tween = target.DOLocalMoveY(toPosition, REPLACE_DURATION).SetAutoKill(true).Play();
			return !isLast ? UniTask.Delay(TimeSpan.FromSeconds(REPLACE_DURATION / 2f)) : tween.ToUniTask();
		}

		private async UniTask SimulateCardShufflingAuidoAsync()
		{
			var cardCount = 2;
			var delayBetwenCardMove = 0.35f;
			for (var i = 0; i < cardCount; i++)
			{
				audioApplication.PlaySound(Clip.Card_Spawn); // its rearrange clip, naming wrong!
				await UniTask.Delay(TimeSpan.FromSeconds(delayBetwenCardMove));
			}
		}

		private async UniTask OnAcceptMulligan()
		{
			mulliganView.SetConfirmButtonVisible(false)
				.SetWaitingOpponentLabelVisible(true)
				.SetCardsContainerInteractable(false);

			foreach (var cardView in gameRepository.CardViews.Where(x => x.IsSelf))
				cardView.SetLock(true);

			var replaceCards = gameRepository.CardViews
				.Where(x => x.MarkedAsSelected)
				.Select(x => x.RuntimeData.Id)
				.ToList();

			var param = new PerformPhaseArgs {Initiator = Executor.RuntimeData.Id, Phase = EffectPhase.InChooseAccepted};
			var model = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash, param)
			{
				ExecutorObjectId = Executor.RuntimeData.Id, 
				TargetObjectsIds = replaceCards
			};
			await gameHub.PerformCommandAsync<PerformPhaseCmd>(model);
		}
		
		private void TryDisplayWaitOpponent()
		{
			var player = gameContext.PlayerRepository.Get(Executor.RuntimeData.OwnerUserId);
			var opponent = gameContext.PlayerRepository.GetOpposite(player.UserId);
			var waitOpponent = player.RuntimeData.IsFinishedMulligan 
			                   && !opponent.RuntimeData.IsFinishedMulligan
			                   && gameContext.Timer.RuntimeData.State < TimerState.Ready;
			
			mulliganView.SetWaitingOpponentLabelVisible(waitOpponent);
		}
	}
}