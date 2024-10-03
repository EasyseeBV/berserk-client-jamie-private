using BerserkV3.Common.TutorialSystem;
using DG.Tweening;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IGameView
	{
		Transform GameInteractionContainer { get; }
		Transform SelfHandContainer{ get; }
		Transform OpponentHandContainer { get; }
		Transform OpponentSpawnContainer { get; }
		Transform SelfSpawnContainer { get; }
		RectTransform TableContainer { get; }
		RectTransform InShowFirstRow { get; }
		RectTransform InShowSecondRow { get; }
		RectTransform SelfHeroContainer { get; }
		RectTransform OpponentHeroContainer { get; }
		Transform VfxContainer { get; }

		void SetActiveteYourTurn(bool value);

		void SetActiveCommendText(bool value);

		void SetCommendText(string value);

		void AnimateTurn();

		void AnimateCommend();

		void SetPlayFieldBlockRaycast(bool value);
	}
	
	public partial class GameView : BaseView, IGameView
	{
		[SerializeField] private CanvasGroup playFieldCanvasGroup;
		
		private Tween turnAnimationTween;
		public Transform GameInteractionContainer => GameContainer;
		public Transform SelfHandContainer => HandSelfContainer;
		public Transform OpponentHandContainer => HandOpponentContainer;
		public Transform OpponentSpawnContainer => OpponentSpawnPoint;
		public Transform SelfSpawnContainer => SelfSpawnPoint;
		public RectTransform TableContainer => PlayFieldContainer;
		public RectTransform InShowFirstRow => inShowFirstRow;
		public RectTransform InShowSecondRow => inShowSecondRow;
		public RectTransform SelfHeroContainer => SelfHeroPoint;
		public RectTransform OpponentHeroContainer => OpponentHeroPoint;
		public Transform VfxContainer => PlayFieldContainer;

		protected override void OnAwake()
		{
			base.OnAwake();
			TutorialGameBoardRect.SetHintTarget($"{TutorialTrigger.GameBoard}").SetTransitionFactorSize().Init();
			HandSelfContainer.SetHintTarget($"{TutorialTrigger.GameHandSelf}").SetTransitionFactorSize().Init();
			SetActiveCommendText(false);
		}

		public void SetActiveteYourTurn(bool value)
		{
			SetActive(YourTurnPanelOverlay, value);
		}

		public void SetActiveCommendText(bool value)
		{
			SetActive(CommendLevel, value);
		}

		public void SetCommendText(string value)
		{
			Set(CommendLevel, value);
		}

		public void AnimateCommend()
		{
			CommendLevel.DOFade(0f, 5f)
				.From(1f)
				.OnComplete(() => SetActiveCommendText(false))
				.Play();
		}

		public void SetPlayFieldBlockRaycast(bool value)
		{
			playFieldCanvasGroup.blocksRaycasts = value;
		}

		public void AnimateTurn()
		{
			turnAnimationTween?.Kill();
			YourTurnPanelOverlay.alpha = 0;
			
			var sequence = DOTween.Sequence();
			sequence.Append(YourTurnPanelOverlay.DOFade(1, 0.5f).SetEase(Ease.InOutQuad));
			sequence.Append(YourTurnPanelOverlay.DOFade(0, 0.5f).SetEase(Ease.InOutQuad).SetDelay(0.5f));
			turnAnimationTween = sequence;
		}
	}
}