using System.Collections.Generic;
using BestHTTP.SignalRCore.Messages;
using Vulcan.Data;

namespace Vulcan.Network
{
	public class BufferItem
	{
		public Message Message;
		public GameMessage Signal;

		public class Comparer : IComparer<BufferItem>
		{
			public int Compare(BufferItem x, BufferItem y)
			{
				if (ReferenceEquals(x, y)) return 0;
				if (ReferenceEquals(null, y)) return 1;
				if (ReferenceEquals(null, x)) return -1;
				return x.Signal.UtcTimeStamp.CompareTo(y.Signal.UtcTimeStamp);
			}
		}
	}
}