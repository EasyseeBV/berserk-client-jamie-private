using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.Lobby
{
	public class LinkData : IConfigData
	{
		public string Id { get; set; }
		public string Link { get; set; }
	}
}