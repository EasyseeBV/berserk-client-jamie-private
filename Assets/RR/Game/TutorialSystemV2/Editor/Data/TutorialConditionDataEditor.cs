using System;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor.Data
{
	[Serializable]
	public class TutorialConditionDataEditor : TutorialConditionData
	{
		[SerializeField] [JsonIgnore] protected string id = string.Empty;
		[SerializeField] [JsonIgnore] protected string meta = string.Empty;

		public override string Id => id;
		public override string Meta => meta;
	}
}