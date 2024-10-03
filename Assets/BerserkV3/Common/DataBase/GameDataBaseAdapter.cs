using Berserk.Shared.Data.Abstraction;

namespace BerserkV3.Common.DataBase
{
	public class GameDataBaseAdapter
	{
		public static IGameDatabase Instance { get; private set; }
		
		public GameDataBaseAdapter(IGameDatabase gameDatabase)
		{
			Instance = gameDatabase;
		}
	}
}