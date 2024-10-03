using System;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor.Data
{
	[Serializable]
	public class TutorialUnmaskDataEditor : TutorialUnmaskData
	{
		[SerializeField] [JsonIgnore] protected Vector2 position;
		[SerializeField] [JsonIgnore] protected Vector2 size;
		[SerializeField] [JsonIgnore] protected string meta = string.Empty;
		[SerializeField] [JsonIgnore] protected bool hideIfMissTarget = true;
		[SerializeField] [JsonIgnore] protected bool autoRefresh;
		[SerializeField] [JsonIgnore] protected bool fitTargetSize;
		[SerializeField] [JsonIgnore] protected bool fitTargetPosition;
		[SerializeField] [JsonIgnore] protected bool disableReposition;
		[SerializeField] [JsonIgnore] protected bool disableResize;
		
		public override TutorialVector2 Position => position;
		public override TutorialVector2 Size => size;
		public override string Meta => meta;
		public override bool HideIfMissTarget => hideIfMissTarget;
		public override bool DisableReposition => disableReposition;
		public override bool FitTargetPosition => fitTargetPosition;
		public override bool DisableResize => disableResize;
		public override bool FitTargetSize => fitTargetSize;
		public override bool AutoRefresh => autoRefresh;
	}
}