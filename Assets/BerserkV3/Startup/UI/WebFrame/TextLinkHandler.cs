using BerserkV3.Common.LiveLinkRouter;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BerserkV3.Startup.UI
{

	[RequireComponent(typeof(TMP_Text))]
	public class TextLinkHandler : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			if (LiveLinkRouterAdapter.Service == null || !TryGetComponent(out TMP_Text text))
				return;

			// If you are not in a Canvas using Screen Overlay, put your camera instead of null
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, null);
			if (linkIndex != -1) // was a link clicked?
			{
				var linkInfo = text.textInfo.linkInfo[linkIndex];
				Application.OpenURL(linkInfo.GetLinkID());
			}
		}
	}

}