using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Network;
using Cysharp.Threading.Tasks;
using GameCore;
using Lobby;
using RR.Core.Extensions;

namespace BerserkV3.Common.Network
{
	public static class AuthTokenRefresher
	{
		public static async UniTask<bool> RefreshAuthTokenAsync(string token)
		{
			var serializeHelper = SerializeHelperAdapter.Service;
			if (string.IsNullOrWhiteSpace(token) && serializeHelper.HasKey(SerializeKeyHelper.REFRESH_TOKEN))
				token = serializeHelper.Get(SerializeKeyHelper.REFRESH_TOKEN);

			if (string.IsNullOrWhiteSpace(token))
				return false;
			
			var response = await IdentityAPI.PostRefreshToken(token);
			if (!response.IsSuccess)
			{
				RequestAuthorization();
				return false;
			}

			User.Data.AccessToken = response.Data.AccessToken;
			User.Data.RefreshToken = response.Data.RefreshToken;

			if (!serializeHelper.HasKey(SerializeKeyHelper.REMEMBER_CREEDS)) 
				return true;
			
			serializeHelper.Patch(SerializeKeyHelper.ACCESS_TOKEN, User.AccessToken);
			serializeHelper.Patch(SerializeKeyHelper.REFRESH_TOKEN, User.RefreshToken);
			serializeHelper.Save();
			return true;
		}
		
		private static void RequestAuthorization()
		{
			switch (SceneServiceAdapter.Service.Current)
			{
				case Scene.Lobby:
					LobbyBus.OnReauthorizationRequired.Publish("The authorization period has expired, please log in again. Press button to continue.");
					break;
				
				case Scene.Game:
					GameCoreBus.OnReauthorizationRequired.Publish("The authorization period has expired, please log in again. Press button to continue.");
					break;
				
				case Scene.Init:
				case Scene.ShowRoom:
				case Scene.StartUp:
				default:
					DefaultSharedLogger.Log($"[{nameof(AuthTokenRefresher).Orange()}] Not handled scene reauthorization : {SceneServiceAdapter.Service.Current}");
					return;
			}
		}
	}
}