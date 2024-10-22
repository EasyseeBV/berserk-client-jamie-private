using System;
using BerserkV3.Common.Network;
using BerserkV3.Common.UIService;
using BerserkV3.Startup.Network.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace BerserkV3.Init.UI
{
	public class ServerChoiceWindow : UISafeWindowBase
	{
		[SerializeField] protected RectTransform MainPage;
		[SerializeField] protected Button StageButton;
		[SerializeField] protected Button LocalhostButton;
		[SerializeField] protected Button CustomButton;
		[SerializeField] protected Button PublicButton;
		[SerializeField] protected Button ProdButton;
		[SerializeField] protected Button DevButton;
		[SerializeField] protected RectTransform InputPage;
		[SerializeField] protected TMP_InputField CusomInput;
		[SerializeField] protected Button InputSubmitButton;
		
		private static string lastCustomUrl = "https://ccg-berserk-dev-v4.azurewebsites.net/";

		protected override void OnInit()
		{
			SetActive(InputPage, false);
			SetActive(MainPage, true);
			Subscribe(CustomButton, () => SetActive(MainPage, false));
			Subscribe(CustomButton, () => SetActive(InputPage, true));
			Subscribe(InputSubmitButton, () => lastCustomUrl = CusomInput.text);
			Subscribe(InputSubmitButton, () => URLs.SetCustomServerUrl(Region.US, CusomInput.text));
			Subscribe(InputSubmitButton, () => URLs.SetCustomServerUrl(Region.EU, CusomInput.text));
			base.OnInit();
		}

		public override void Hidden()
		{
			SetActive(InputPage, false);
			SetActive(MainPage, true);
			
			if(StageButton)
				StageButton.onClick.RemoveAllListeners();
			
			if(LocalhostButton)
				LocalhostButton.onClick.RemoveAllListeners();
			
			if(CustomButton)
				CustomButton.onClick.RemoveAllListeners();
			
			if(PublicButton)
				PublicButton.onClick.RemoveAllListeners();
			
			if(ProdButton)
				ProdButton.onClick.RemoveAllListeners();
			
			if(DevButton)
				DevButton.onClick.RemoveAllListeners();
			
			if(InputSubmitButton)
				InputSubmitButton.onClick.RemoveAllListeners();
			
			base.Hidden();
		}

		public void SetSelectedAction(Action<Environment> value)
		{
			Subscribe(StageButton, () => value(Environment.Staging));
			Subscribe(ProdButton, () => value(Environment.Production));
			Subscribe(PublicButton, () => value(Environment.Test));
			Subscribe(LocalhostButton, () => value(Environment.LocalHost));
			Subscribe(DevButton, () => value(Environment.Development));
			Subscribe(InputSubmitButton, () => value(Environment.Custom));
		}

		public override void Show()
		{
			CusomInput.text = lastCustomUrl;
			base.Show();
		}

		private void SetActive(Component component, bool value)
		{
			if(component)
				component.gameObject.SetActive(value);
		}

		private void Subscribe(Button button, Action onClick)
		{
			if(button)
				button.onClick.AddListener(() => onClick?.Invoke());
		}
	}
}