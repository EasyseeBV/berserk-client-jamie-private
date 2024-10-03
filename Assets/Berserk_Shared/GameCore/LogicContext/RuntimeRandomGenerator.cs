using System;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class RuntimeRandomGenerator : IRuntimeRandomGenerator
	{
		private Random random;

		public int Seed { get; private set; }
		
		public int Next(int? num = null)
		{
			return num.HasValue ? random.Next(num.Value) : random.Next();
		}

		public void Sync(int value)
		{
			Seed = value;
			random = new Random();
		}
	}
}