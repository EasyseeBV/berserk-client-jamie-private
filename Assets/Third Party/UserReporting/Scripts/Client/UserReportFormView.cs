using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Third_Party.UserReporting.Scripts.Client
{

	public class UserReportFormView : MonoBehaviour
	{
		[SerializeField] private TMP_InputField summary;
		[SerializeField] private TMP_InputField description;
		[SerializeField] private Button submit;

		private void Awake()
		{
			if (summary)
				summary.onValueChanged.AddListener(OnInputValueChanged);

			if (description)
				description.onValueChanged.AddListener(OnInputValueChanged);

			OnInputValueChanged(string.Empty);
		}

		private void OnInputValueChanged(string value)
		{
			var isInteractable = true;
			if (summary)
				isInteractable &= summary.text.Trim().Length > 0;

			if (description)
				isInteractable &= description.text.Trim().Length > 0;

			if (submit)
				submit.interactable = isInteractable;
		}

		private void OnDestroy()
		{
			if (summary)
				summary.onValueChanged.RemoveListener(OnInputValueChanged);

			if (description)
				description.onValueChanged.RemoveListener(OnInputValueChanged);
		}
	}

}