namespace BerserkV3.GameCore.Cards
{
	public interface IGlowView
	{
		void Setup(bool isSelf);
		void Enable(bool value, GlowType type);
		void Disable();
	}
	
	public enum GlowType
	{
		Turn,
		Targeting,
		Selection,
	}
}