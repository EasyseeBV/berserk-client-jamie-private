using System;
using BerserkV3.Common.Network;
using BerserkV3.Common.Network.SignalR;
using BerserkV3.Startup.Authorization;
using BestHTTP.SignalRCore.Messages;
using Events;

namespace BerserkV3.Lobby.Network
{
	public sealed class LobbyHub : BaseSignalHub, ILobbyHub
	{
		protected override string AuthToken => User.AccessToken;
		protected override string ServerAddress => URLs.HubUrl;
		public override string HubName => "lobby";
		
		private bool HandleInternalSignalTypes(Message message)
		{
			if (message.type is MessageTypes.Ping or MessageTypes.Completion)
				return true;

			switch (message.type)
			{
				case MessageTypes.Ping or MessageTypes.Completion : 
					return true;
				
				case MessageTypes.Close : 
					RestartRequired();
					return true;
				
				default: return false;
			}
		}

		protected override void HandleMessage(Message message)
		{
			try
			{
				if (HandleInternalSignalTypes(message))
					return;
				
				base.HandleMessage(message);
			}
			catch (Exception e)
			{
				ErrorDispatcher.OnException += e;
			}
		}
	}
}