using Berserk.Shared.Data.Enums;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public class HeroHolderViewOpponent : BaseHeroHolderView
	{
		[SerializeField] protected RawImage DisconnectIndicator;
		[SerializeField] protected RawImage BotIndicator;

		public override Owner ViewOwner => Berserk.Shared.Data.Enums.Owner.Opponent;

		public override void Setup()
		{
			base.Setup();
			DisconnectIndicator.LoadResourceAsync("Vulcanite_Indicator_Disconnect").Forget();
			BotIndicator.LoadResourceAsync("Vulcanite_Indicator_Bot").Forget();
		}

		public override void SetIsDisconnected(bool value)
		{
			SetActive(DisconnectIndicator, value);
		}

		public override void SetIsBot(bool value)
		{
			SetActive(BotIndicator, value);
		}
	}
}