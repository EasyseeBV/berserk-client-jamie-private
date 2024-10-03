using BerserkV3.Common.UIKit.KeyboardHeightService;
using TMPro;
using UnityEngine.EventSystems;

namespace BerserkV3.Common.UIKit
{
	public class TextInputField : TMP_InputField
	{
		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			KeyboardHeightServiceAdapter.Service?.SetFocuse(m_RectTransform);
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			KeyboardHeightServiceAdapter.Service?.Release(m_RectTransform);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			KeyboardHeightServiceAdapter.Service?.Release(m_RectTransform);
		}
	}
}