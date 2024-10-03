using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Third_Party.UserReporting.Scripts.Client
{

	public class UserReportCapchaView : MonoBehaviour
	{
		[SerializeField] private UserReportingScript reportSystem;
		[SerializeField] private CapchaRenderer rendererPrefab;
		[SerializeField] private TMP_InputField codeInput;
		[SerializeField] private RawImage codeImage;
		[SerializeField] private Button submit;
		[SerializeField] private Button cancel;
		private Texture defaultCapchaImage;
		private Color defaultColor;
		private Color wrongColor;
		private Capcha current;

		private void Awake()
		{
			defaultColor = codeInput.targetGraphic.color;
			wrongColor = Color.red;
			wrongColor.a = defaultColor.a;
			submit.onClick.AddListener(ValidateInput);
			cancel.onClick.AddListener(Close);
			codeInput.onSubmit.AddListener(_ => ValidateInput());
			codeInput.onValueChanged.AddListener(_ => SetInputColor(defaultColor));
			defaultCapchaImage = codeImage.texture;
			Close();
		}

		public void Show()
		{
			ResetView();
			SetCapchaAsync().Forget();
			gameObject.SetActive(true);
		}

		private async UniTask SetCapchaAsync()
		{
			EnableInput(false);
			var capchaRenderer = Instantiate(rendererPrefab);
			current = await capchaRenderer.CreateAsync();
			codeImage.texture = current.Texture;
			EnableInput(true);
			Destroy(capchaRenderer.gameObject);
		}

		private void ValidateInput()
		{
			if (current.IsExpired)
			{
				ResetView();
				SetCapchaAsync().Forget();
				return;
			}

			if (!current.Validate(codeInput.text))
			{
				SetInputColor(wrongColor);
				return;
			}

			EnableInput(false);
			reportSystem.SubmitUserReport();
			Close();
		}

		private void Close()
		{
			SetInputColor(defaultColor);
			current.Release();
			ResetView();
			gameObject.SetActive(false);
		}

		private void SetInputColor(Color color)
		{
			codeInput.targetGraphic.color = color;
		}

		private void EnableInput(bool value)
		{
			codeInput.interactable = value;
			submit.interactable = value;
			cancel.interactable = value;
		}

		private void ResetView()
		{
			codeImage.texture = defaultCapchaImage;
			codeInput.text = string.Empty;
		}

		private void OnDestroy()
		{
			if (submit)
				submit.onClick.RemoveAllListeners();

			if (cancel)
				cancel.onClick.RemoveAllListeners();

			if (codeInput)
				codeInput.onSubmit.RemoveAllListeners();

			if (codeInput)
				codeInput.onValueChanged.RemoveAllListeners();
		}
	}

}