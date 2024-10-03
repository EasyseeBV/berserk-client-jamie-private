using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.Cards
{

	public partial class CardBuildingLayout : CardCreatureLayout
	{
		protected override async UniTask OnEnabledAsync(CancellationToken token)
		{
			await base.OnEnabledAsync(token);
			SetActiveArmor(true);
		}
		
		protected override void OnArmorChanged(int from, int to)
		{
			if (IsAllowedExternal)
				return;
			
			SetActiveArmor(to > 0);
			SetArmor(to);
		}

		protected override void OnAttackChanged(int from, int to)
		{
			if (IsAllowedExternal)
				return;
			
			SetActiveAttack(to > 0);
			SetAttack(to);
		}

		protected override void OnHpChanged(int from, int to)
		{
			if (IsAllowedExternal)
				return;
			
			SetActiveHealth(to > 0);
			SetHealth(to);
		}
	}
}