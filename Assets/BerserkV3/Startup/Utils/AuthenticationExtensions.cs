using System;
using Berserk.Shared.Data.Identity.Social;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Events;
using UnityEngine;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace BerserkV3.Startup.Utils
{
	public static class AuthenticationExtensions
	{
		public static bool IsSocialsAvailable(this ExternalProvider[] providers)
		{
			return StartupBus.TestFly || EnvironmentSwitcher.CurrentEnvironment is not Environment.Test
				&& providers is {Length: > 0};
		}
		
		public static bool IsPlatformAvailable(this ExternalProvider provider) => provider switch
		{
			ExternalProvider.Google => true,
			ExternalProvider.Facebook => true,
			ExternalProvider.Twitter => true,
			ExternalProvider.TikTok => true,
			ExternalProvider.Discord => true,
			ExternalProvider.Apple => Application.platform == RuntimePlatform.IPhonePlayer
			                          || Application.platform == RuntimePlatform.OSXPlayer,
			_ => throw new NotImplementedException($"Unknown {nameof(ExternalProvider)} : {provider}")
		};
	}
}