using System;
using Berserk.Shared.Data.Identity.Social;
using RR.Core.Extensions;
using RR.Core.Serialization;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;

namespace BerserkV3.Common.UIKit
{
	public partial class SocialWidget : BaseView
	{
		[Serializable]
		protected class SocialIcons : UnitySerializedDictionary<ExternalProvider, Sprite> {}
		[SerializeField] protected ComplexButton ButtonItem;
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected SocialIcons IconsProvider;
		
		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		private void OnDestroy()
		{
			Clear();
		}

		public void SetText(string value)
		{
			Set(HeaderText, value);
		}
		

		public void SetActive(bool value)
		{
			SetActive(this, value);
		}

		public ComplexButton[] GetAllButtins()
		{
			return transform.GetComponentsInChildren<ComplexButton>();
		}
		
		public ComplexButton CreateButton(ExternalProvider provider)
		{
			if (!IconsProvider.ContainsKey(provider))
				throw new ArgumentException($"Provider doesn't exist in collection : {provider}");
			
			SetActive(ButtonItem, true);
			var button = Instantiate(ButtonItem, transform);
			button.SetIcon(IconsProvider[provider]);
			SetActive(ButtonItem, false);
			return button;
		}

		public void Clear()
		{
			GetComponentsInChildren<ComplexButton>(true).ForEach(button =>
			{
				if (button == ButtonItem)
					return;
				
				button.Clear();
				Destroy(button.gameObject);
			});
		}
	}
}