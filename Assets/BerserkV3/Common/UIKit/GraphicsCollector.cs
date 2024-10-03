using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public class GraphicsCollector : MonoBehaviour
	{
		[SerializeField] private bool includeParent;
		[SerializeField] private bool includeInactive;
		[SerializeField] private Graphic[] targetGraphics;

		public Graphic[] TargetGraphics => targetGraphics == null
			? Array.Empty<Graphic>()
			: targetGraphics.ToArray();

		[ContextMenu("Collect Graphics")]
		private void Collect()
		{
			var searchTarget = includeParent && transform.parent
				? transform.parent
				: transform;
			targetGraphics = searchTarget.GetComponentsInChildren<Graphic>(includeInactive);
		}
	}
}