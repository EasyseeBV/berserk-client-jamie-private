using Berserk.Shared.Data.Enums;
using Cysharp.Threading.Tasks;
using GameCore;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public class HeroHolderViewSelf : BaseHeroHolderView
	{
		[SerializeField] protected Button emotionsButton;
		[SerializeField] protected RawImage emotionsButtonImage;

		public override Owner ViewOwner => Berserk.Shared.Data.Enums.Owner.Self;
		
		public override void Setup()
		{
			base.Setup();
			var lifeTimeToken = this.GetCancellationTokenOnDestroy();
			emotionsButton.onClick.AddListener(OnEmotionButtonClick);
			emotionsButtonImage.LoadResourceAsync("Vulcanite_Emotions_Button", lifeTimeToken).Forget();
			GameCoreBus.OnRequestedChatWheel.SubscribeFirst(this, OnEmotionInteractableChanged).CallWhenInactive();
		}

		public override void SetIsDisconnected(bool value){}
		public override void SetIsBot(bool value){}
		
		private void OnEmotionButtonClick()
		{
			if (GameCoreBus.OnRequestedChatWheel.Value)
				return;

			GameCoreBus.OnRequestedChatWheel.Publish(true);
		}

		private void OnEmotionInteractableChanged(bool value)
		{
			emotionsButton.interactable = !value;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (emotionsButton)
				emotionsButton.onClick.RemoveAllListeners();
			
			emotionsButtonImage.ReleaseResource();
		}
	}
}