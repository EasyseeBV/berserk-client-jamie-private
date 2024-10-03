using System;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.Cards
{
	[Serializable]
	public class GlowItem
	{
		public GlowType Type;
		public Graphic[] Graphics;
	}
	
	public class GlowView : MonoBehaviour, IGlowView
	{
		[SerializeField] private GlowItem[] glowItems;
		private GlowSettings glowSettings;
		private bool isSelf;

		private void Awake()
		{
			glowSettings = Resources.Load<GlowSettings>(nameof(GlowSettings));
			Disable();
		}

		public void Setup(bool isSelf)
		{
			this.isSelf = isSelf;
		}

		public void Enable(bool value, GlowType type)
		{
			var glowItem = glowItems?.FirstOrDefault(x => x.Type == type);
			if (glowItem == null)
			{
				RRLogger.Log($"Glow {nameof(Graphic)} {"does not exist or missing ".Red().Bold()} : {type}");
				return;
			}

			EnableInternal(glowItem, value);
		}

		public void Disable()
		{
			glowItems?.ForEach(glowItem => EnableInternal(glowItem, false));
		}

		private void EnableInternal(GlowItem item, bool enable)
		{
			foreach (var graphic in item.Graphics)
			{
				graphic.gameObject.SetActive(enable);
				graphic.color = GetColor(item.Type, graphic);
			}
		}

		private Color GetColor(GlowType type, Graphic graphic)
		{
			switch(type)
			{
				case GlowType.Turn : return graphic.color; 
				case GlowType.Targeting : return isSelf ? glowSettings.SelfTargetingColor : glowSettings.OpponentTargetingColor;
				case GlowType.Selection : return isSelf ? glowSettings.SelfSelectionColor : glowSettings.OpponentSelectionColor;
				default: throw new NotImplementedException($"[{GetType().Name.Orange()}] Unknown {nameof(GlowType)} : {type}");
			};
		}
	}
}