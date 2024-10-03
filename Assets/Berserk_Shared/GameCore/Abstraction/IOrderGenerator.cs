namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IOrderGenerator
	{
		int Current { get; }

		void Sync(int value);
		int Next();
	}
}