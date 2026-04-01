using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
	public partial class ButtonView : BaseView
	{
		[Header("Custom View")]
		public Image CustomView;
		public Button Button { get; private set; }

		protected void Awake()
		{
			Button = GetComponent<Button>();
			Button.targetGraphic = BackgroundImage;
		}

		public Button Get()
		{
			return Button;
		}

		public void Subscribe(UnityAction onClick)
		{
			if (Button == null)
				Button = GetComponent<Button>();

			Button.onClick.AddListener(onClick);
		}
		
		public void UnSubscribe(UnityAction onClick)
		{
			if (Button == null)
				Button = GetComponent<Button>();

			Button.onClick.RemoveListener(onClick);
		}
		
		public void UnSubscribeAll()
		{
			if (Button == null)
				Button = GetComponent<Button>();

			Button.onClick.RemoveAllListeners();
		}

		public void SetInteractable(bool value)
		{
			if (Button == null)
				Button = GetComponent<Button>();
			Button.interactable = value;
		}

		private void OnDestroy()
		{
			UnSubscribeAll();
		}
	}
}