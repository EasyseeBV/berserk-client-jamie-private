using TMPro;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public class HintView : MonoBehaviour
	{
		[SerializeField] protected TextMeshProUGUI TitleText;
		[SerializeField] protected TextMeshProUGUI DescriptionText;
		
		public string Id { get; private set; }
		public Transform SelfContainer { get; private set; }

		public void Setup()
		{
			if (!SelfContainer)
				SelfContainer = transform;
			
			SetPosition(Vector3.zero);
			SetSize(Vector3.one);
		}

		public void SetTitle(string value)
		{
			TitleText.SetText(value ?? string.Empty);
		}

		public void SetDescription(string value)
		{
			DescriptionText.text = value;
			DescriptionText.SetText(value ?? string.Empty);
		}

		public void SetParent(Transform value, bool worldPositionStays = false)
		{
			SelfContainer.SetParent(value, worldPositionStays);
		}

		public void SetPosition(Vector3 value)
		{
			SelfContainer.localPosition = value;
		}

		public void SetSize(Vector3 value)
		{
			SelfContainer.localScale = value;
		}

		public void SetId(string value)
		{
			Id = value;
		}
	}
}