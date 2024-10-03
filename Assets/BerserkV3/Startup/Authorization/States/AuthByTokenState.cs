using System;
using System.Net;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Network;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using RR.Core.Extensions;

namespace BerserkV3.Startup.Authorization
{
	public class AuthByTokenState : State
	{
		private readonly ISerializeHelper serializeHelper;

		public AuthByTokenState(ISerializeHelper serializeHelper)
		{
			this.serializeHelper = serializeHelper;
		}

		public override async void OnEnter(params object[] args)
		{
			try
			{
				if (!serializeHelper.HasKey(SerializeKeyHelper.ACCESS_TOKEN))
					throw new UnauthorizedAccessException($"[{GetType().Name.Orange()}] User unauthorized. User doesn't have available token.");
			
				var response = await IdentityAPI.PostAuthenticate(serializeHelper.Get(SerializeKeyHelper.ACCESS_TOKEN)).AddLoadingTask();
				if (response.Code != HttpStatusCode.OK || response.Data == null)
					throw new Exception($"[{GetType().Name.Orange()}] Authorization by token failed. {response.GetMessage()}");
				
				var signInArgs = new AuthProcessingArgs {AuthModel = response.Data};
				StateMachineBus.Switch<AuthProcessingState>(signInArgs);
			}
			catch (Exception e)
			{
				User.LogOut();
				DefaultSharedLogger.Log(e.Message);
				StateMachineBus.Switch<AuthSignInState>();
			}
		}
	}
}