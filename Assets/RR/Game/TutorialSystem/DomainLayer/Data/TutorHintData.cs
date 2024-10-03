using System;
using UnityEngine;

namespace RR.Game.TutorialSystem.Domain.Data
{
	[Serializable]
	public class TutorHintData
	{
		public string Id { get; set; }

		/// <summary>
		///     Completion and saves only these hints.
		///     If you want to complete and save immediately, then duplicate the id of the current hint
		/// </summary>
		public string[] DependentHintsIdForCompletion { get; set; } = Array.Empty<string>();

		public string[] RequiredCompletedHintIds { get; set; } = Array.Empty<string>();
		public string NextTutorHintId { get; set; }
		public string[] Meta { get; set; } = { "" };
		public float DelayBeforeInvoke { get; set; }
		public float DelayAfterInvoke { get; set; }
		public bool RequiredPause { get; set; }
		public bool[] IsUnmasks { get; set; } = { false };
		public bool IsNextButtonShown { get; set; } = true;
		public string PlaceUIName { get; set; }

		public string Title { get; set; }
		public string Text { get; set; }
		public Vector3[] Offsets { get; set; } = Array.Empty<Vector3>();
		public Vector3 PopupPosition { get; set; } = Vector3.zero;
		public Vector2[] FixedRects { get; set; } = Array.Empty<Vector2>();
		public bool IsAttach { get; set; }
		public bool IsPopupArrowShown { get; set; }
		public bool IsPointerArrow { get; set; }
		public object PointerArrowFrom { get; set; }
		public Vector3 PointerArrowFromOffset { get; set; } = Vector3.zero;
		public object PointerArrowTo { get; set; }
		public Vector3 PointerArrowToOffset { get; set; } = Vector3.zero;
		public bool IsNeedSyncToServer { get; set; }
		public bool IsRequiredBackHider { get; set; } = true;
	}
}