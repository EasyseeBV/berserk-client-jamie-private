using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class RuntimeIdGenerator : IRuntimeIdGenerator
	{
		public int Current { get; private set; }

		public int Next()
		{
			return Current++;
		}

		public void Sync(int value)
		{
			if (Current >= value)
				return;

			Current = value;
		}
	}
}