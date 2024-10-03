using TMPro;
using UnityEngine;

namespace Assets.RR.UI
{
	[RequireComponent(typeof(TMP_Text))]
	public class SemVerListener : MonoBehaviour
	{
		[SerializeField] private string prefix = "v";
		[SerializeField] private string postfix = default;
		[SerializeField, HideInInspector] private TMP_Text txt = default;

		private void Awake()
		{
			txt.SetText($"{prefix}{Application.version}{postfix}");
		}

#if UNITY_EDITOR
		private void Reset()
		{
			txt = GetComponent<TMP_Text>();
		}
#endif
	}
}
