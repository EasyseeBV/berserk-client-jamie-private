using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.RuntimeObjects.Events
{
	public class DamageEventArgs
	{
		public IRuntimeGameObject Source { get; }
		public IRuntimeGameObject Target { get; }
		public bool IsAlive { get; }
		public int Value { get; }
		public DamageType DamageType { get; }

		public DamageEventArgs(IRuntimeGameObject source, IRuntimeGameObject target, bool isAlive, int value, DamageType damageType)
		{
			Source = source;
			Target = target;
			IsAlive = isAlive;
			Value = value;
			DamageType = damageType;
		}
	}
}