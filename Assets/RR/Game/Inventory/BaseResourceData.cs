using RR.Core;
using System;

namespace RR.Game.Inventory
{
	[Serializable]
	public abstract class BaseResourceData
	{
		public string Id;
		public string Type;
		public DoubleStat Amount;

		public override string ToString()
			=> $"<b>{Id}<b>:{Amount:####}";
	}
}