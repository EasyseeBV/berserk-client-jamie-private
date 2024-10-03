using System;
using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	[Serializable]
	public class ColorHdr
	{
		[ColorUsage(true, true)]
		[SerializeField]
		private Color color;

		public Color Color => color;
	}
}