namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeStateController
	{
		void Process(IRuntimeGameCard target);
	}
}