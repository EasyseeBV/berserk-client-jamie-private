using System;
using System.Linq;
using BerserkV3.Startup;
using BerserkV3.Startup.Events;
using BerserkV3.Startup.Network.Enums;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace BerserkV3.Common.Network
{
	[DefaultExecutionOrder(-100)]
	public sealed class EnvironmentSwitcher
	{
		public static Environment CurrentEnvironment => selected ?? Initialize();

		public static Region CurrentRegion { get; private set; }

		public static event Action OnSwitch;

		private static Environment? selected;

		public static Environment Initialize()
		{
#if PRODUCTION
			var target = Environment.Production;
#elif RC
			var target = Environment.ReleaseCandidate;
#elif PUBLIC_TEST
			var target = Environment.Test;
#else
			var target = Environment.Staging;
#endif
			if (!selected.HasValue || selected != target)
				SwitchEnvironment(target);
			
			return target;
		}

		public static void SwitchEnvironment(Environment env)
		{
			selected = env;
			RRLogger.Log($"[{nameof(EnvironmentSwitcher).Orange()}] Switch Environment to {env}");
			StartupBus.TestFly.Assign(StartupBus.TestFly || env < Environment.Test);
			OnSwitch?.Invoke();
		}

		public static void SwitchRegion(Region region)
		{
			CurrentRegion = region;
			RRLogger.Log($"Switch Region to {region}");
		}

		public static string GetCurrentVersion()
		{
			var environmentText = selected is Environment.Production
				? ""
				: $"{selected?.ToString().FirstOrDefault()}";

			return $"{UnityEngine.Application.version}{environmentText} {CurrentRegion}";
		}
	}
}