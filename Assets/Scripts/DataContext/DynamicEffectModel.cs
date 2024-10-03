using Berserk.Shared.Data.Enums;
using Vulcan.Data;

namespace ServerCore.Infrastructure.Models
{
	public class DynamicEffectModel
	{
		public string Id;
		public EffectKeyword Effect;
		public string EffectId;
		public int Length;
		public int Value;
		public string[] Params;
	}
}