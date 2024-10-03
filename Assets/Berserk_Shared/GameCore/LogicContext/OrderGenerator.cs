using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class OrderGenerator : IOrderGenerator
	{
		public int Current { get; private set; }

		public void Sync(int value)
		{
			Current = value;
		}

		public int Next()
		{
			Current++;
			return Current;
		}
	}
}