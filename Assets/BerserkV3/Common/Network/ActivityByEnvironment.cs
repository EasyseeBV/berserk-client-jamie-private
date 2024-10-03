using BerserkV3.Startup.Events;
using BerserkV3.Startup.Network.Enums;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.Common.Network
{
	public class ActivityByEnvironment : MonoBehaviour
	{
		[SerializeField] private Environment environment = Environment.Staging;
		[SerializeField] private bool forceInfluenceBaseView;
		private bool IsActive => StartupBus.TestFly || EnvironmentSwitcher.CurrentEnvironment <= environment;

		private void Start()
		{
			EnvironmentSwitcher.OnSwitch += Setup;
			StartupBus.TestFly.SubscribeRaw(EnableTestFly);
			Setup();
		}

		private void EnableTestFly(bool value)
		{
			Setup();
		}

		private void OnDestroy()
		{
			EnvironmentSwitcher.OnSwitch -= Setup;
			StartupBus.TestFly.Unsubscribe(EnableTestFly);
		}

		private void Setup()
		{
			if (forceInfluenceBaseView && TryGetComponent(out BaseView view))
				view.ShowAtStart = IsActive;
			
			gameObject.SetActive(IsActive);
			RRLogger.Log($"[{GetType().Name.Orange()}/{gameObject.name.Orange()}] IsActive : {IsActive}, by environment : {EnvironmentSwitcher.CurrentEnvironment}, ");
		}
	}
}