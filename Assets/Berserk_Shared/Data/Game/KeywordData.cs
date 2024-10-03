using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.Game
{
	public class KeywordData: IConfigData
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}
}