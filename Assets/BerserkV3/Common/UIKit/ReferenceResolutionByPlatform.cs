using System;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	[DefaultExecutionOrder(-1)]
	[RequireComponent(typeof(CanvasScaler))]
	public class ReferenceResolutionByPlatform : MonoBehaviour
	{
		[Serializable]
		private class ReferencePlatforms : UnitySerializedDictionary<RuntimePlatform, Vector2>{}

		[SerializeField] private ReferencePlatforms referencePlatforms = new();
		private void Awake()
		{
			if (!referencePlatforms.TryGetValue(Application.platform, out var referenceResolution))
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] Not implemented {nameof(RuntimePlatform)} : {Application.platform}");
				return;
			}

			GetComponent<CanvasScaler>().referenceResolution = referenceResolution;
		}
	}
}