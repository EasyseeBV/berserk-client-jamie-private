using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.GameCore.Customisations;
using BerserkV3.Generic.Customisation;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.GameCore.Cards
{
	public partial class HeroLayout : CardCreatureLayout
	{
		protected override string TurnShineArtUrl => $"Vulcanite_Shine";
		protected override string BorderArtUrl => GetFrameArtUrl();
		protected override string MaskArtUrl => $"Vulcanite_Mask";
		protected override string HealthArtUrl => $"Vulcanite_Attribute_Health";
		protected override string AttackArtUrl => $"Vulcanite_Attribute_Attack";
		protected override string ArmorArtUrl => $"Vulcanite_Attribute_Armor";
		
		protected override void SetupHintTargets()
		{
			HealthStatImage.SetHintTarget($"{TutorialTrigger.HeroHealth}_{GetOwner()}").SetTransitionFactorSize().Init();
			AttackStatImage.SetHintTarget($"{TutorialTrigger.HeroAttack}_{GetOwner()}").SetTransitionFactorSize().Init();
			ArmorStatImage.SetHintTarget($"{TutorialTrigger.HeroArmor}_{GetOwner()}").SetTransitionFactorSize().Init();
			selfContainer.SetHintTarget($"{TutorialTrigger.HeroBounds}_{GetOwner()}").SetTransitionFactorSize().Init();
		}

		private string GetFrameArtUrl()
		{
			var assetData = GameCustomisationsAdapter.Application
				.Get<AssetData>(RuntimeData.OwnerUserId, CustomisationType.AvatarFrame);

			return assetData?.URL ?? base.BorderArtUrl;
		}
		
		protected override void OnAttackChanged(int from, int to)
		{
			base.OnAttackChanged(from, to);
			
			if (IsAllowedExternal)
				return;
			
			SetActiveAttack(to > 0);
		}
	}

}