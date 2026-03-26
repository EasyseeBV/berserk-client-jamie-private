using Berserk.Shared.GameCore.Commands;
using Berserk.Shared.GameCore.Models;
using BerserkV3.Common.Network.SignalR;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.Network.Abstraction
{
	public interface IGameHub : ISignalHub
	{
		UniTask PerformCommandAsync<T>(CmdParamsModel model = null, bool predict = false) where T : Command;
		UniTask AutoPerformCommandAsync();
		UniTask ReadyToInitializeAsync();
	}
}
