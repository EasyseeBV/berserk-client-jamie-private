using Berserk.Shared.GameCore.Abstraction;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public abstract class LogicEvent : ILogicEvent
	{
		public bool NotPredictable { get; }
		public bool IsForceSync { get; }
		public ILogicEvent Reverse()
		{
			throw new System.NotImplementedException();
		}

		public int Order { get; set; }
		
		public override string ToString()
		{
			return $"{GetType().Name} {JsonConvert.SerializeObject(this)}";
		}
	}
}