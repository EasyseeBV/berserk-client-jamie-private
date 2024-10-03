using System;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.TutorialSystem;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public interface IHeroHolderView
	{
		Transform AbilityView { get; }
		ILavaView LavaView { get; }
		Owner ViewOwner { get; }
		event Action OnAbilityClick;

		void Setup();
		string GetUserNameText();
		void SetUserNameText(string value, int maxLenght = 16);
		void SetAbiltyText(string value);
		void SetAbilityButtonActive(bool value);
		void SetAbiltyButtonInteractable(bool value);
		void SetIsBot(bool value);
		void SetIsDisconnected(bool value);
	}
	
	public abstract class BaseHeroHolderView : BaseView, IHeroHolderView
	{
		[SerializeField] protected LavaView lavaView;
		[SerializeField] protected TextMeshProUGUI nameText;
		[SerializeField] protected TextMeshProUGUI abilityText;
		[SerializeField] protected RawImage abilityButtonImage;
		[SerializeField] protected Button abilityButton;

		public Transform AbilityView => abilityButton && abilityButton.transform ? abilityButton.transform : null;
		public ILavaView LavaView => lavaView;
		public abstract Owner ViewOwner { get; }
		public event Action OnAbilityClick;

		private bool initialized;

		protected override void OnAwake()
		{
			base.OnAwake();
			lavaView.SetHintTarget($"{TutorialTrigger.GameLava}_{ViewOwner}").SetTransitionFactorSize().Init();
		}

		public virtual void Setup()
		{
			if (initialized)
				return;

			initialized = true;
			var lifeTimeToken = this.GetCancellationTokenOnDestroy();
			abilityButtonImage.LoadResourceAsync("Vulcanite_Ability_Button", lifeTimeToken).Forget();
			abilityButton.onClick.AddListener(() => OnAbilityClick?.Invoke());
		}

		public void SetUserNameText(string value, int maxLenght = 16)
		{
			if (!nameText)
				return;

			if (string.IsNullOrEmpty(value))
				value = "Unknown";

			Set(nameText, value.Length > maxLenght ? value.Substring(0, maxLenght) : value);
		}

		public string GetUserNameText()
		{
			return !nameText ? "Not set yet." : nameText.text;
		}

		public void SetAbiltyText(string value)
		{
			if (!abilityText)
				return;

			Set(abilityText, value);
		}

		public void SetAbilityButtonActive(bool value)
		{
			if (!abilityButton)
				return;

			SetActive(abilityButton, value);
		}

		public void SetAbiltyButtonInteractable(bool value)
		{
			if (abilityButton)
				SetInteractable(abilityButton, value);

			if (!abilityText)
				return;

			var color = abilityText.color;
			color.a = value ? 1f : 0.5f;
			abilityText.color = color;
		}

		public abstract void SetIsBot(bool value);

		public abstract void SetIsDisconnected(bool value);

		protected virtual void OnDestroy()
		{
			if (abilityButton)
				abilityButton.onClick.RemoveAllListeners();

			abilityButtonImage.ReleaseResource();
			OnAbilityClick = null;
		}
	}
}