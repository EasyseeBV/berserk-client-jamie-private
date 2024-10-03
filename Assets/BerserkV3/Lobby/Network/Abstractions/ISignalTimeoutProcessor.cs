using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Network
{
	public interface ISignalTimeoutProcessor
	{
		int TimeoutMs { get; set; }
		int TimeoutResponseBufferMs { get; set; }
		bool Enabled { get; set; }
		
		void ResponseReceived(object target);
		UniTask<bool> WaitResponseAsync(object target, bool useResponseBuffer = false);
		void Clear();
	}
}