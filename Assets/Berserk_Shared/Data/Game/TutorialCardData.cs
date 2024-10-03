using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.Data.Game
{
	public class TutorialCardData : IConfigData
	{
		public string Id { get; set; }
		public string CardId { get; set; }

		public override string ToString()
		{
			return this.ReflectionFormat();
		}
	}
}