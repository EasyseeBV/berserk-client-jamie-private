using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	/// <inheritdoc />
	/// <summary>
	/// Old way to play single VFX (compare to complex visual effect sequences)
	/// </summary>
	#region General

	[EffectVisual(EffectVisualKeyword.BuffAttack)]
	[EffectVisual(EffectVisualKeyword.BuffHealth)]

	#endregion General

	#region Keyword
	
	[EffectVisual(EffectVisualKeyword.Heal)]
	[EffectVisual(EffectVisualKeyword.Reborn)]
	[EffectVisual(EffectVisualKeyword.Summon)]
	
	#endregion Keywords

	#region Cards
	
	[EffectVisual(EffectVisualKeyword.Buff)]
	[EffectVisual(EffectVisualKeyword.CardSpawn)]
	[EffectVisual(EffectVisualKeyword.DestroyCard_HA)]
	[EffectVisual(EffectVisualKeyword.Exile)]
	[EffectVisual(EffectVisualKeyword.LifeSteal)]
	[EffectVisual(EffectVisualKeyword.Rage)]
	[EffectVisual(EffectVisualKeyword.Raving)]
	[EffectVisual(EffectVisualKeyword.Slash)]
	[EffectVisual(EffectVisualKeyword.Blacksmith)]
	[EffectVisual(EffectVisualKeyword.Vendetta)]
	[EffectVisual(EffectVisualKeyword.Hex)]
	[EffectVisual(EffectVisualKeyword.Parry)]
	[EffectVisual(EffectVisualKeyword.Empower)]
	[EffectVisual(EffectVisualKeyword.SunfireStrike)]
	
	#endregion

	#region Heroes

	[EffectVisual(EffectVisualKeyword.DamageOpponent_HA)]
	[EffectVisual(EffectVisualKeyword.DamageRandomCard_HA)]
	[EffectVisual(EffectVisualKeyword.Disarm_HA)]
	[EffectVisual(EffectVisualKeyword.EvilEye_HA)]
	[EffectVisual(EffectVisualKeyword.ExtraHealth_HA)]
	[EffectVisual(EffectVisualKeyword.Hex_HA)]
	[EffectVisual(EffectVisualKeyword.SilenceHero_HA)]
	[EffectVisual(EffectVisualKeyword.Silence_HA)]
	[EffectVisual(EffectVisualKeyword.SpellDamage_HA)]
	[EffectVisual(EffectVisualKeyword.SwapStats)]
	[EffectVisual(EffectVisualKeyword.Undefeatable_HA)]
	[EffectVisual(EffectVisualKeyword.DamageAllCards_HA)]

	#endregion

	public class DefaultSingleVfxVisual : EffectVisual
	{
		public override UniTask PlaySingleEffectAsync()
		{
			return StartEffectAsync();
		}

		public override UniTask StartEffectAsync()
		{
			using var db = DisposeBlock.Spawn();
			var animations = db.SpawnList<UniTask>();

			animations.AddRange(Targets.Select(target =>
				VfxApplication.PlaySingleVFXAsync(EffectData.VisualKeyword, target.SelfContainer)));

			return UniTask.WhenAll(animations);
		}

		public override void Dispose()
		{
		}
	}
}