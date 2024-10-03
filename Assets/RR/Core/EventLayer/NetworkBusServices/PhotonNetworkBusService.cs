//-----------------------------------------------------------------
// Example class of implementation INetworkBusService for Photon.
//-----------------------------------------------------------------


#if PHOTON_UNITY_NETWORKING

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;

namespace RR.Core.EventLayer
{
    /// <summary>
    /// Implements INetworkBusService for Photon.
    /// Requires installed Photon.Pun.
    /// 
    /// Just drag this script on some GameObject. It will automatically enable Networked Events.
    /// </summary>
    class PhotonNetworkBusService : MonoBehaviourPunCallbacks, IOnEventCallback, INetworkBusService
    {
        public event Action<int, byte[]> OnEventCallback;

        void Awake()
        {
            BusToNetworkBridge.Init<Bus>(this);
        }

        public void RaiseEvent(int eventId, byte[] data)
        {
            byte evCode = (byte)eventId;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(evCode, data, raiseEventOptions, sendOptions);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.CustomData == null || photonEvent.CustomData is byte[])
                OnEventCallback?.Invoke(photonEvent.Code, (byte[])photonEvent.CustomData);
        }
    }
}
#endif