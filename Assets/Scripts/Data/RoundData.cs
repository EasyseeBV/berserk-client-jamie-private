using System;
using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Vulcan.Data
{
	[Serializable]
	public class RoundData
	{
		public int RoundNumber = 1;
		public Owner TurnOwner;
		public DateTime TurnTimerEndsOn;

		public override string ToString()
		{
			return string.Join(",", GetType().GetFields().Select(x => $"[{x.Name}={x.GetValue(this)}]"));
		}
	}
}