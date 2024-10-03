using System;
using System.Linq;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor.Data
{
	[Serializable]
	public class TutorialBlockEditor : TutorialBlock
	{
		[SerializeField] [JsonIgnore] protected string id = string.Empty;
		[SerializeField] [JsonIgnore] protected TutorialBlockDataEditor data = new();
		[SerializeField] [JsonIgnore] protected TutorialPopupDataEditor popup = new();
		[SerializeField] [JsonIgnore] protected TutorialPointerDataEditor pointer = new();
		[SerializeField] [JsonIgnore] protected TutorialConditionDataEditor[] conditions = Array.Empty<TutorialConditionDataEditor>();
		[SerializeField] [JsonIgnore] protected TutorialUnmaskDataEditor[] unmasks = Array.Empty<TutorialUnmaskDataEditor>();
		[SerializeField] [HideInInspector] [JsonIgnore] public string defaultId;

		public override string Id => string.IsNullOrEmpty(id) ? defaultId : id;
		public override TutorialBlockData Data => data;
		public override TutorialPopupData Popup => popup;
		
		/// <summary>
		/// If we don't want to use a pointer, we will write the json as null.
		/// </summary>
		public override TutorialPointerData Pointer => pointer.Enabled ? pointer : null;

		/// <summary>
		/// If we don't want to use any conditions, we will write the json as null.
		/// </summary>
		public override TutorialConditionData[] Conditions => conditions.Length > 0
			? conditions.OfType<TutorialConditionData>().ToArray()
			: null;

		/// <summary>
		/// If we don't want to use any unmask, we will write the json as null.
		/// </summary>
		public override TutorialUnmaskData[] Unmasks => unmasks.Length > 0
			? unmasks.OfType<TutorialUnmaskData>().ToArray()
			: null;
	}
}