using RR.Core.EventLayer;
using Vulcan.Data;

namespace Events
{
	public sealed class DataBus : EventBus
	{
		public static State<AppData> AppData = new SerializedState<AppData>(new AppData());
	}
}