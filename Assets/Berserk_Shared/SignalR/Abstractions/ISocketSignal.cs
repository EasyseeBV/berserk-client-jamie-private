using Berserk.Shared.SignalR.Enums;

namespace Berserk.Shared.SignalR.Abstractions
{
	public interface ISocketSignal
	{
		SignalType Type { get; }
	}
}