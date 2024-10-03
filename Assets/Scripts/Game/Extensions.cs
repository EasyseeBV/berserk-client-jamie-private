using System.Collections.Generic;
using System.Linq;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
	public static class Extensions
	{
		public static void DestroyImmediateSafe(this Texture texture)
		{
			if (texture == null)
				return;
			
			if (!texture.name.StartsWith(PicLoader.NamePrefix))
				return;

			Object.DestroyImmediate(texture);
		}

		public static bool IsControlledOnClient(this SessionPlayerModel playerModel)
		{
			return false;

			//return playerModel != null && playerModel.ConnectionStatus == (byte)ConnectionStatus.Disconnected
			//	&& playerModel.UserName != "BotPlayer";
		}
		
		public static bool IsDisconnected(this SessionPlayerModel playerModel)
		{
			return playerModel == null || playerModel.ConnectionStatus == PlayerConnectionStatus.Disconnected;
		}
	}
}