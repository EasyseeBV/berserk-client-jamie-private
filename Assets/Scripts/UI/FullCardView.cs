using System;
using System.Threading;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Components;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public enum HorizontalShift
	{
		Right,
		Left,
		TopRight,
		Leftmost,
		None
	}

	[Obsolete("Use preview system instead")]
	public class FullCardView : Singleton<FullCardView>
	{
		//TODO: remove hard code

		private static readonly float MAX_Y = 378f;
		private static readonly float MIN_Y = 216f;
		private static readonly float SLIGHT_OFFSET_X = 140f;
		private static readonly float OFFSET_LEFTMOST_X = -480f;
		private static readonly float RIGHT_ROW_POSITION_X = 378f;

		private static readonly Vector3 SLIGHT_OFFSET_TOP_RIGHT = new Vector3(SLIGHT_OFFSET_X, SLIGHT_OFFSET_X, 0f);
		private static readonly Vector3 LEFT_LOCAL_POSITION = new Vector3(-1123f, 0, 0f);

		private static readonly Vector3 DEFAULT_LOCAL_SCALE = Vector3.one;
		private static readonly Vector3 ENLARGED_SCALE_LOCAL_SCALE = DEFAULT_LOCAL_SCALE * 2;

		[SerializeField] private CardHandArtView handCardArtView = default;
		[SerializeField] private CanvasGroup canvasGroup = default;

		private CancellationTokenSource setupArt;
		private CardData currentData;
		private static bool IsShown => Instance.canvasGroup.alpha != 0;

		// use Canvas Scaler;
		// UI Scale Mode: Scale with screen size;
		// Reference resolution: 1000 x 600;
		// Screen match mode: Expand;

		private CanvasScaler canvasScaler;
		private float Rate => (Instance.canvasScaler?.referenceResolution.y ?? Screen.height) / Screen.height;

		protected override void OnAwake()
		{
			canvasScaler = FindObjectOfType<CanvasScaler>();
			Instance.canvasGroup.alpha = 0;
			Instance.handCardArtView.SetActive(false);
			Application.focusChanged+= ApplicationOnfocusChanged;
		}

		public static void Show(CardData cardData)
		{
			if (IsShown && Instance.currentData.Id.Same(cardData.Id))
				return;

			Instance.currentData = cardData;
			Instance.canvasGroup.alpha = 1;
			Instance.InterruptSetupArt();
			Instance.setupArt = new CancellationTokenSource();
			
			Instance.handCardArtView
				.SetupAsync(cardData.ToCardDataAdapter(), Instance.setupArt.Token)
				.ContinueWith(() =>
				{
					Instance.handCardArtView.SetActiveShine(false);
					Instance.handCardArtView.SetActive(true);
				})
				.Forget();
		}

		public static void SetWarning(bool value)
		{
			Instance.handCardArtView.SetWarning(value);
			Instance.handCardArtView.SetTransparency(1f);
		}

		public static void Hide()
		{
			if (!IsShown)
				return;

			Instance.canvasGroup.alpha = 0;
			Instance.handCardArtView.SetActive(false);
		}

		public static void SetPoisition(Transform transformSelectCard, HorizontalShift horizontalShiftDirection = default)
		{
			if (horizontalShiftDirection == default)
				horizontalShiftDirection = GetDefaultHorizontalShiftDirection(transformSelectCard);

			var newPosition = GetNewPosition(transformSelectCard, horizontalShiftDirection);
			Instance.handCardArtView.RectTransform.position = newPosition;
		}

		public static void SetPosition(HorizontalShift horizontalShiftDirection = HorizontalShift.None)
		{
			switch (horizontalShiftDirection)
			{
				case HorizontalShift.Leftmost:
					Instance.handCardArtView.RectTransform.localPosition = LEFT_LOCAL_POSITION;
					Instance.handCardArtView.RectTransform.localScale = DEFAULT_LOCAL_SCALE;
					break;

				case HorizontalShift.None:
					Instance.handCardArtView.RectTransform.localPosition = Vector3.zero;
					Instance.handCardArtView.RectTransform.localScale = ENLARGED_SCALE_LOCAL_SCALE;
					break;
			}
		}

		private static HorizontalShift GetDefaultHorizontalShiftDirection(Transform transformSelectCard)
		{
			return transformSelectCard.localPosition.x >= RIGHT_ROW_POSITION_X
				? HorizontalShift.Left
				: SystemInfo.deviceType == DeviceType.Handheld ? HorizontalShift.TopRight : HorizontalShift.Right;
		}

		private static Vector3 GetNewPosition(Transform transformSelectCard, HorizontalShift horizontalShiftDirection)
		{
			Vector3 offset;
			switch (horizontalShiftDirection)
			{
				case HorizontalShift.Right:
					var offSetX = SLIGHT_OFFSET_X / Instance.Rate;
					offset = new Vector3(offSetX, 0f, 0f);
					break;

				case HorizontalShift.Left:
					offSetX = -SLIGHT_OFFSET_X / Instance.Rate;
					offset = new Vector3(offSetX, 0f, 0f);
					break;

				case HorizontalShift.Leftmost:
					offSetX = OFFSET_LEFTMOST_X;
					offset = new Vector3(offSetX, 0f, 0f);
					break;

				case HorizontalShift.TopRight:
					offset = SLIGHT_OFFSET_TOP_RIGHT / Instance.Rate;
					break;

				default:
					offset = Vector3.zero;
					break;
			}

			var newPosition = transformSelectCard.position + offset;

			if (newPosition.y > MAX_Y / Instance.Rate)
			{
				newPosition.y = MAX_Y / Instance.Rate;
			}
			else if (newPosition.y < MIN_Y / Instance.Rate)
			{
				newPosition.y = MIN_Y / Instance.Rate;
			}

			return newPosition;
		}

		private void InterruptSetupArt()
		{
			setupArt?.Cancel();
			setupArt?.Dispose();
			setupArt = null;
		}
		
		private void ApplicationOnfocusChanged(bool focus)
		{
			if(!focus)
				Hide();
		}
		
		private void OnDestroy()
		{
			Application.focusChanged -= ApplicationOnfocusChanged;
		}
	}
}
