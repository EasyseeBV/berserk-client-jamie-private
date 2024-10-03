using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;

namespace Berserk.Shared.GameCore.EffectSystem
{
	public interface IEffectsFactory
	{
		KeywordEffect Create(EffectKeyword keyword);
		IEffectCondition Create(EffectConditionType type);
	}

	public class EffectsFactory : IEffectsFactory
	{
		protected readonly TypeCollection<EffectKeyword, EffectKeywordAttribute> EffectTypes;
		protected readonly TypeCollection<EffectConditionType, EffectConditionAttribute> ConditionTypes;

		public EffectsFactory()
		{
			EffectTypes = new TypeCollection<EffectKeyword, EffectKeywordAttribute>(att => att.Keyword, typeof(KeywordEffect));
			ConditionTypes = new TypeCollection<EffectConditionType, EffectConditionAttribute>(att => att.ConditionType, typeof(EffectConditionBase));
		}

		public virtual KeywordEffect Create(EffectKeyword keyword)
		{
			if (!EffectTypes.TryGet(keyword, out var effectType))
				throw new ArgumentNullException($"{nameof(keyword)} {keyword} not found");

			if (effectType == null)
				throw new ArgumentNullException(
					$"There is no {nameof(keyword)} class type matching the effect {keyword}!");

			return (KeywordEffect)Activator.CreateInstance(effectType);
		}

		public IEffectCondition Create(EffectConditionType type)
		{
			if (!ConditionTypes.TryGet(type, out var conditionType))
				throw new ArgumentNullException($"{nameof(type)} {type} not found");

			if (conditionType == null)
				throw new ArgumentNullException(
					$"There is no {nameof(type)} class type matching the effect {type}!");

			return (IEffectCondition)Activator.CreateInstance(conditionType);
		}
	}
}