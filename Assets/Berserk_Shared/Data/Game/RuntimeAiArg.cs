namespace Berserk.Shared.Data.Game
{
	public class RuntimeAiArg
	{
		public string Id { get; set; }
		public bool Expired { get; set; }
		public int Cycle { get; set; } = 1;
	}
}