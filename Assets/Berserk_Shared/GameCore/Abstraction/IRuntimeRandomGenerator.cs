namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeRandomGenerator
	{
		int Seed { get; }
		int Next(int? num = null);
		void Sync(int value);
	}
}