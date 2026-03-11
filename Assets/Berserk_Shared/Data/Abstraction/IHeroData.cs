namespace Berserk.Shared.Data.Abstraction
{
	public interface IHeroData : IObjectData
	{
		int LevelAtSync { get; set; }
		int LevelAtRegistration { get; set; }
		string Name { get; set; }
		int AbilityManaCost { get; set; }
		int AbilityHpCost { get; set; }
	}
}