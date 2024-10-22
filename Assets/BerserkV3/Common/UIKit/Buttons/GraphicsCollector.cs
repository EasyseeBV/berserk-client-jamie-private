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

		public Graphic[] TargetGraphics => targetGraphics ?? Array.Empty<Graphic>();
		
		public Graphic this[int i]
		{
			get => targetGraphics[i];
			set => targetGraphics[i] = value;
		}

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