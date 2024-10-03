using System;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor.Data
{
	[Serializable]
	public class TutorialPopupDataEditor : TutorialPopupData
	{
		[SerializeField] [JsonIgnore] [TextArea(1, 5)] protected string title = string.Empty;
		[SerializeField] [JsonIgnore] [TextArea(2, 5)] protected string text = string.Empty;
		[SerializeField] [JsonIgnore] protected string continueButtonText = string.Empty;
		[SerializeField] [JsonIgnore] protected Vector2 position;
		[SerializeField] [JsonIgnore] protected Vector2 sizeDelta;
		[SerializeField] [JsonIgnore] protected float delayBeforShow;
		[SerializeField] [JsonIgnore] protected bool arrowRequired;
		[SerializeField] [JsonIgnore] protected bool continueButton = true;
		[SerializeField] [JsonIgnore] protected bool backButton = true;
		[SerializeField] [JsonIgnore] protected bool backHinder = true;
		[SerializeField] [JsonIgnore] protected bool autoSize = true;
		[SerializeField] [JsonIgnore] protected bool enabled = true;

		public override string Title => title;
		public override string Text => text;
		public override string ContinueButtonText => continueButtonText;
		public override TutorialVector2 Position => position;
		public override TutorialVector2 SizeDelta => sizeDelta;
		public override float DelayBeforShow => delayBeforShow;
		public override bool ArrowRequired => arrowRequired;
		public override bool ContinueButton => continueButton;
		public override bool BackButton => backButton;
		public override bool BackHinder => backHinder;
		public override bool AutoSize => autoSize;
		public override bool Enabled => enabled;
	}
}