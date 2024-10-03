using NUnit.Framework;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.UI.Tests
{
	public class UIManagerTest
	{
		private GameObject uiManager;

		[OneTimeSetUp]
		public void PrepareTestEnvironment()
		{
			RRLogger.Log("Preparing test env.");

			uiManager = new GameObject("UI_MANAGER (Test)", typeof(UIManager), typeof(Canvas));
			uiManager.SendMessage("Reset");

#if UNITY_INCLUDE_TESTS

#else
			RRLogger.Error($"Tests cant not be properly initialized caused by DEFINE[UNITY_INCLUDE_TESTS] not set.");
#endif
		}

		[OneTimeTearDown]
		public void DropTestEnvironment()
		{
			Object.Destroy(uiManager);

			RRLogger.Log("Drop test env.");
		}

		private void EndTestAndCleanUp()
		{
			uiManager.transform.DestroyChildren();
			uiManager.SendMessage("Reset");
		}
	}
}