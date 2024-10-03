using Newtonsoft.Json;
using RR.Core;

namespace Vulcan.Data
{
	public class CardStat : IntStat
	{
		[JsonConstructor]
		public CardStat(object max) : base(max)
		{
		}

		public CardStat(int max = 0) : base(max)
		{
		}

		public CardStat(int current = 0, int max = 0) : base(current, max)
		{
		}

		public CardStat(CardStat copy) : base(copy)
		{
		}

		public new int GetDefault => Default;

		public int GetPrevious => Previous;

		public void AddAboveMax(int value)
		{
			SetAboveMax(Current + value);
		}
	}
}