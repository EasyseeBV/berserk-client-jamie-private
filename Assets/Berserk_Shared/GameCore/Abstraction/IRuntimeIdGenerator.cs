namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeIdGenerator
	{
		int Current { get; }
		int Next();
		void Sync(int value);
	}
}