using System;
using Berserk.Shared.SignalR.Abstractions;
using Berserk.Shared.SignalR.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.SignalR.Common
{
	public class SocketSignal<T> : ISocketSignal
    {
	    public SignalType Type { get; private set; }
        public T Message { get; set; }
        public Type DataType { get; set; }
        public long UtcTimeStamp { get; protected set; }

        public SocketSignal(SignalType type, T msg)
        {
            Type = type;
            Message = msg;
            DataType = typeof(T);
            ResetTimeStamp();
        }

        public void ResetTimeStamp()
        {
            UtcTimeStamp = DateTimeOffset.UtcNow.ToFileTime();
        }

        public override string ToString() => JsonConvert.SerializeObject(this, SignalUtils.SOCKET_SIGNAL_SETTINGS);
    }
}
