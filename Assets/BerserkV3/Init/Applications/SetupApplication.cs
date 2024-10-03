using System;
using System.Threading.Tasks;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using RR.Core.DebugSystem;
using UnityEngine;
using Zenject;

namespace BerserkV3.Init.Applications
{
	public class SetupApplication : DisposableWithCts, IInitializable
	{
		private readonly ISceneService sceneService;
		public SetupApplication(ISceneService sceneService)
		{
			this.sceneService = sceneService;
		}

		public void Initialize()
		{
			sceneService.OnSceneLoaded += OnSceneLoaded;
			OnSceneLoaded(Scene.Init);
		}

		private async void OnSceneLoaded(Scene loadded)
		{
			try
			{
				await Task.Delay(TimeSpan.FromSeconds(0.05f));
				Application.targetFrameRate = Application.isMobilePlatform ? 70 : 145;
				Screen.sleepTimeout = SleepTimeout.NeverSleep;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public override void Dispose()
		{
			sceneService.OnSceneLoaded -= OnSceneLoaded;
			base.Dispose();
		}
	}
}