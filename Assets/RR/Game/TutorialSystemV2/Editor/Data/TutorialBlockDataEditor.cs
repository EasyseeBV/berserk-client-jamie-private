using System;
using System.Linq;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor.Data
{
	[Serializable]
	public class TutorialBlockDataEditor : TutorialBlockData
	{
		[SerializeField] [JsonIgnore] protected TutorialStepEditor nextBlock;
		[SerializeField] [JsonIgnore] protected float delayBeforeInvoke;
		[SerializeField] [JsonIgnore] protected float delayAfterClose;
		[SerializeField] [JsonIgnore] protected bool pauseRequired;
		[SerializeField] [JsonIgnore] protected bool isSyncRequired = true;
		[SerializeField] [JsonIgnore] protected string[] keywordTriggers = Array.Empty<string>();
		[SerializeField] [JsonIgnore] protected TutorialStepEditor[] requiredCompletedHints = Array.Empty<TutorialStepEditor>();
		[SerializeField] [JsonIgnore] protected TutorialStepEditor[] completeDependedHints = Array.Empty<TutorialStepEditor>();

		public override string NextId => nextBlock ? nextBlock.Id : string.Empty;
		public override float DelayBeforeInvoke => delayBeforeInvoke;
		public override float DelayAfterClose => delayAfterClose;
		public override bool PauseRequired => pauseRequired;
		public override bool IsSyncRequired => isSyncRequired;
		public override string[] KeywordTriggers => keywordTriggers;
		public override string[] RequiredCompletedHints => requiredCompletedHints.Select(x => x.Id).ToArray();
		public override string[] CompleteDependedHints => completeDependedHints.Select(x => x.Id).ToArray();
	}
}