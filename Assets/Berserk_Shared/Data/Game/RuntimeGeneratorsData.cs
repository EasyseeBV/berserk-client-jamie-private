using System;
using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.Game
{
	[Serializable]
	public class RuntimeGeneratorsData : IRuntimeDataBase
	{
		public int IdGenerator { get; set; } = -1;
		public int OrderGenerator { get; set; } = 0;
		public int RandomGenerator { get; set; }
	}
}