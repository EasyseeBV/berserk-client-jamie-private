using RR.UI.FrameSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

namespace Global
{
	public class View : BaseView
	{
		protected event Action OnEscape;

		protected bool Initialized;

		[Tooltip("Doesn't work on mobile devices")]
		[SerializeField] private bool navigatable = false;

		public virtual void BlockUI(bool block)
		{
			if(CanvasGroup)
				CanvasGroup.interactable = !block;
		}

		public override void Initialize()
		{
			if (Initialized)
				return;

			Initialized = true;

			base.Initialize();
		}

		protected override void OnShown()
		{
			if (SystemInfo.deviceType == DeviceType.Handheld)
			{
				navigatable = false;
			}
			if (navigatable)
			{
				var firstSelectable = GetComponentsInChildren<Selectable>().First();
				EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject, new BaseEventData(EventSystem.current));
			}
			base.OnShown();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				OnEscape?.Invoke();

			if (!navigatable || !Input.GetKeyDown(KeyCode.Tab))
				return;

			var next = EventSystem.current.currentSelectedGameObject?
				.GetComponent<Selectable>()?
				.FindSelectableOnDown();

			if (next == null)
				return;

			EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
		}
	}
}