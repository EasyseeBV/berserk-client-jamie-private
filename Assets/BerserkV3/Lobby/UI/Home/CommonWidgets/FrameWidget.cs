using System;
using System.Threading;
using BerserkV3.Common.UIKit;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class FrameWidget : UIViewBase
	{
		[SerializeField] private Image artImage;
		private bool releasePrevious;
		
		public async UniTask InitAsync(string artUrl, CancellationToken token = default)
		{
			try
			{
				await artImage.LoadResourceAsync(artUrl, token, releasePrevious);
				releasePrevious = true;
			}
			catch (Exception ex)
			{
				RRLogger.Error($"Error in {nameof(InitAsync)} loading art from URL: {artUrl}. Exception: {ex.Message}");
			}
		}

		public void Clear()
		{
			if(releasePrevious)
				artImage.ReleaseResource();

			releasePrevious = false;
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}