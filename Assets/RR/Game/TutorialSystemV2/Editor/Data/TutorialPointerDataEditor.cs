using System;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor.Data
{
	[Serializable]
	public class TutorialPointerDataEditor : TutorialPointerData
	{
		[SerializeField] [JsonIgnore] protected Vector3 startPosition;
		[SerializeField] [JsonIgnore] protected Vector3 endPosition;
		[SerializeField] [JsonIgnore] protected bool enabled;

		public override TutorialVector3 StartPosition => startPosition;
		public override TutorialVector3 EndPosition => endPosition;
		[JsonIgnore] public bool Enabled => enabled;
	}
}