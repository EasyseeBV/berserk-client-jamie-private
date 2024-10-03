using Berserk.Shared.Data.Enums;
using Game.Entities;
using Vulcan.Data;
using EffectData = Vulcan.Data.EffectData;

namespace Events
{
	/// <summary>
	/// Used by <see cref="GameBus.PlaceEffect"/>
	/// </summary>
	public class EffectInfo
	{
		public IMonoEntity From;
		public IMonoEntity Target;

		public EffectKeyword Effect;
		public EffectVisualKeyword VisualEffect;

		public EffectInfo()
		{
		}

		public EffectInfo(IMonoEntity target, EffectData effectData)
		{
			From = target;
			Target = target;
			Effect = effectData.Effect;
			VisualEffect = effectData.VisualEffect;
		}

		public override string ToString()
		{
			return $"{From?.DataBase.Id} -> {Target?.DataBase.Id}: ({VisualEffect}({Effect}))";
		}
	}
}