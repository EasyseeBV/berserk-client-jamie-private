using System.Linq;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Editor.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor
{
	[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/TutorialStep", order = 1)]
	public class TutorialStepEditor : ScriptableObject
	{
		[SerializeField] [JsonIgnore] protected TutorialBlockEditor hint;
		public TutorialBlockEditor Hint => hint;

		public string Id => hint.Id;

		public TutorialStepEditor RefreshHint()
		{
			hint.defaultId = null;
			if (string.IsNullOrEmpty(hint.Id))
				hint.defaultId = name;

			return this;
		}

		public int GetJsonOrder()
		{
			if (string.IsNullOrEmpty(Id))
				return int.MaxValue;

			return int.TryParse(string.Join("", Id.Where(char.IsDigit)), out var order) ? order : int.MaxValue;
		}
	}
}