using System;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Abstraction
{
	public class ImmuneKeyword
	{
		public string Keyword { get; }
		public int EffectId { get; }

		[JsonConstructor]
		public ImmuneKeyword(string keyword, int effectId)
		{
			EffectId = effectId;
			Keyword = keyword;
		}

		public override bool Equals(object obj)
		{
			if (obj is string keyword)
				return Keyword == keyword;
			
			return obj is ImmuneKeyword other 
			       && other.Keyword == Keyword
			       && other.EffectId == EffectId;
		}
		
		public override int GetHashCode()
		{
			return HashCode.Combine(Keyword, EffectId);
		}

		public override string ToString()
		{
			return $"EffectId:{EffectId}, Keyword:{Keyword}";
		}
	}
}