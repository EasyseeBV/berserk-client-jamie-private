using System.Collections.Generic;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Vulcan.Data;

namespace UI
{
	public partial class CardTooltipPanel : BaseView
	{
		private Transform attachTarget;
		private Vector3 attachOffset;
		private string debugTitle = "DEBUG".Red();
		private bool isDebug;
		private HashSet<VisualEffect> spawnerTooltips = new HashSet<VisualEffect>();
		
		private const float offsetX = 50f;

		private static readonly Regex splitEffectRE = new Regex(@"(\B[A-Z])", RegexOptions.Compiled);

		protected override void Start()
		{
			base.Start();

			DescriptionPanel.gameObject.SetActive(false);
		}

		public CardTooltipPanel SetUp(params EffectDescription[] effects)
		{
			transform.DestroyChildrenExcept(DescriptionPanel.transform);
			spawnerTooltips.Clear();
			
			effects.ForEach(x =>
			{
				if (spawnerTooltips.Contains(x.Effect))
					return;
				
				TitleTxt.SetText(EffectToString(x.Effect));
				TooltipTxt.SetText(x.Description);
			
				var panel = Instantiate(DescriptionPanel.gameObject, transform);
				spawnerTooltips.Add(x.Effect);
			
				panel.SetActive(true);
			});
			
			return this;
		}

		public void SetPositionForMobile(bool isMobileDevice)
		{
			if (isMobileDevice)
			{
				transform.localPosition = new Vector3(-offsetX, transform.position.y, transform.position.z);
			}
		}

		public CardTooltipPanel RemoveDebug()
		{
			if (!isDebug)
				return this;

			var debugPanel = transform
				.GetChildren<TMP_Text>()
				.FirstOrDefault(x => x.text == debugTitle);

			if (debugPanel != null)
				Destroy(debugPanel.gameObject);

			return this;
		}

		// public CardTooltipPanel SetDebug(EntityState state)
		// {
		// 	isDebug = true;
		// 	RemoveDebug();
		//
		// 	TitleTxt.SetText(debugTitle);
		// 	TooltipTxt.SetText(state.ToString());
		// 	var debug = Instantiate(DescriptionPanel.gameObject, transform);
		// 	debug.SetActive(true);
		// 	debug.transform.SetAsFirstSibling();
		//
		// 	return this;
		// }

		public CardTooltipPanel AttachTo(Transform target, Vector3 offset)
		{
			attachTarget = target;
			attachOffset = offset;

			return this;
		}

		private void LateUpdate()
		{
			if (attachTarget == null)
				return;

			transform.position = attachTarget.position + attachOffset;
		}

		private string EffectToString(VisualEffect effect)
		{
			var effectText = splitEffectRE.Replace(effect.ToString(), " $1");
			var index = effectText.IndexOf('_');
			if (index > 0)
				effectText = effectText.Substring(0, index);

			return effectText;
		}
	}
}