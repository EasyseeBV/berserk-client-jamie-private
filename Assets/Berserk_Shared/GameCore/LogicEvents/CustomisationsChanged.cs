using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class CustomisationsChanged : LogicEvent
	{
		public string UserId { get; }
		public string EquippedCustomItems { get; }

		[JsonConstructor]
		public CustomisationsChanged(string userId, string equippedCustomItems)
		{
			UserId = userId;
			EquippedCustomItems = equippedCustomItems;
		}
	}
}