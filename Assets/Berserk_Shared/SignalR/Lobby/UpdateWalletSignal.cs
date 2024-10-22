using Berserk.Shared.Data.Identity;
using Berserk.Shared.SignalR.Common;
using Berserk.Shared.SignalR.Enums;

namespace Berserk.Shared.SignalR.Lobby
{
	public class UpdateWalletSignal : SocketSignal<UserDataModel>
	{
		public UpdateWalletSignal(UserDataModel msg) : base(SignalType.Store, msg)
		{
		}
	}
}