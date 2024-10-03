using TMPro;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual
{
	public class VFXText : VFXView
	{
		[SerializeField] private TextMeshProUGUI fxText;
		[SerializeField] private Vector2 sortingOffsets;
		private Transform selfCached;
		protected override void Awake()
		{
			selfCached = transform;
			base.Awake();
		}

		public override void SetupDetph()
		{
			base.SetupDetph();
			if (onTop) selfCached.SetAsLastSibling();
			else selfCached.SetAsFirstSibling();
			
			var pos = selfCached.localPosition;
			pos.z += onTop ? sortingOffsets.y : sortingOffsets.x;
			selfCached.localPosition = pos;
		}

		public VFXText SetText(string value)
		{
			fxText.SetText(value ?? string.Empty);
			return this;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			selfCached = null;
		}
	}
}