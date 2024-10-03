using BerserkV3.Startup.Events;
using TMPro;
using UI;
using UnityEngine;

namespace BerserkV3.Common.Network
{

	[RequireComponent(typeof(TMP_InputField))]
	public class TestFly : MonoBehaviour
	{
		private float actionTimeEnd;
		private const string KEYWORD = "rrtest";
		
		private void OnEnable()
		{
			if (Cleanup())
				return;
			
			if (TryGetComponent(out TMP_InputField inputField))
				inputField.onValueChanged.AddListener(OnInputChanged);

			actionTimeEnd = (Application.isMobilePlatform ? KEYWORD.Length * 2 : KEYWORD.Length) + Time.realtimeSinceStartup;
			if (!Application.isMobilePlatform)
				inputField.Select();
		}

		private void OnDisable()
		{
			Unscribe();
		}

		private void OnDestroy()
		{
			Unscribe();
		}

		private void Unscribe()
		{
			if (!TryGetComponent(out TMP_InputField inputField)) 
				return;
			
			inputField.onValueChanged.RemoveListener(OnInputChanged);
			inputField.SetTextWithoutNotify(string.Empty);
		}

		private bool Cleanup()
		{
			if (!StartupBus.TestFly)
				return false;
			
			Destroy(this);
			return true;
		}

		private void OnInputChanged(string value)
		{
			if (actionTimeEnd < Time.realtimeSinceStartup || value != KEYWORD)
				return;

			StartupBus.TestFly += true;
			ConfirmationDialog.Instance.Init()
				.SetMessage("Congratulations, you have gained access to the testing functions.")
				.SetTitle("Information")
				.SetCancel()
				.Apply();
			
			Cleanup();
		}
	}

}