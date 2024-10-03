using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.TooltipPopup.Data
{
	public class TooltipEffectData
	{
		public int Value {get; set;}
		public int Length {get; set;}
		public string Title {get; set;}
		public string Description {get; set;}
		public EffectKeyword Keyword {get; set;}
		public string EffectConfigId {get; set;}
		public string IconUrl {get; set;}
	}
}