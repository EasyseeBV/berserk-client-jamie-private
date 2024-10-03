using Berserk.Shared.Data.Abstraction;

namespace BerserkV3.Common.DataBase
{
	public class SharedConfigAdapter
	{
		public static ISharedConfig Config { get; private set; }
		
		public SharedConfigAdapter(ISharedConfig config)
		{
			Config = config;
		}
	}
}