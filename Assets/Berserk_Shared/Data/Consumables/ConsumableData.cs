using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.Consumables
{
	public class ConsumableData : IConsumableData, IConfigData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ArtUrl { get; set; }
	}
}