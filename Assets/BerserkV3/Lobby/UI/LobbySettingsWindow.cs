using System;
using RR.UIService;
using RR.UIService.FullFade;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI
{
	public class LobbySettingsWindow : UIWindowBase, IFullFadeTarget
	{
		[SerializeField] private Slider soundSlider;
		[SerializeField] private Slider musicSlider;
		[SerializeField] private Button tutorialButton;
		[SerializeField] private Button cancelButton;
		[SerializeField] private Button closeButton;
		[SerializeField] private Button applyButton;

		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}

		public void SetSound(float initValue, Action<float> onChanged = null)
		{
			if (!soundSlider)
				return;

			soundSlider.SetValueWithoutNotify(initValue);
			
			if (onChanged != null)
				soundSlider.onValueChanged.AddListener(onChanged.Invoke);
		}

		public void SetMusic(float initValue, Action<float> onChanged = null)
		{
			if (!musicSlider)
				return;

			musicSlider.SetValueWithoutNotify(initValue);
			
			if (onChanged != null)
				musicSlider.onValueChanged.AddListener(onChanged.Invoke);
		}

		public void SetCloseAction(Action value)
		{
			if (closeButton)
				closeButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetCancelAction(Action value)
		{
			if (cancelButton)
				cancelButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetApplyAction(Action value)
		{
			if (applyButton)
				applyButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetTutorialAction(Action value)
		{
			if (tutorialButton)
				tutorialButton.onClick.AddListener(() => value?.Invoke());
		}

		public void Clear()
		{
			if (cancelButton)
				cancelButton.onClick.RemoveAllListeners();

			if (closeButton)
				closeButton.onClick.RemoveAllListeners();

			if (applyButton)
				applyButton.onClick.RemoveAllListeners();

			if (tutorialButton)
				tutorialButton.onClick.RemoveAllListeners();

			if (soundSlider)
				soundSlider.onValueChanged.RemoveAllListeners();

			if (musicSlider)
				musicSlider.onValueChanged.RemoveAllListeners();
		}

		public Color? FadeColor => null;
		public void OnFadeClick(){}
	}
}