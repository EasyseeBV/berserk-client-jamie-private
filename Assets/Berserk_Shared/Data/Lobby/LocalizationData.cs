using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.Lobby
{
	public class LocalizationData : IConfigData
	{
		public string Id { get; set; }
		public string Text { get; set; }
		
		public static implicit operator string(LocalizationData data)
		{
			return data.Text;
		}
	}
}