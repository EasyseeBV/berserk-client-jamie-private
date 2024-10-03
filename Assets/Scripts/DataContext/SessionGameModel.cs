using System;
using System.Linq;

namespace ServerCore.Infrastructure.Models
{
	[Obsolete("Client not used this model")]
	public abstract class SessionGameModel
	{
		public string SessionPlayerId { get; set; }
		
		public override string ToString()
		{
			return string.Join("\n", GetType().GetFields().Select(x => $"[{x.Name} : {x.GetValue(this)}]"));
		}
	}
}