using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Network.Enums;
using UnityEngine;

namespace BerserkV3.GameCore.Controllers
{
	public static class LocalAutoPilot
	{
		private static bool? enabled;

		public static bool Enabled
		{
			get => IsAvailable && (enabled ??= ShouldDefaultToEnabled());
			set => enabled = value;
		}

		public static bool IsAvailable =>
			Application.isEditor &&
			EnvironmentSwitcher.CurrentEnvironment == Environment.LocalHost;

		private static bool ShouldDefaultToEnabled()
		{
			return IsAvailable;
		}

		public static void ResetForSession()
		{
			enabled = null;
		}
	}
}
