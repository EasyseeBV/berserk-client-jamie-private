using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Startup.UI
{
	public class AuthSocialResponseWindow : UISafeWindowBase
	{
		[SerializeField] protected Image CircleLoading;
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI MessageText;
		[SerializeField] protected ComplexButton CancelButton;
		[SerializeField] private float rotationAmount = -360f;
		[SerializeField] private float duration = 2f;
		[SerializeField] private Ease easeType = Ease.Linear;

		private Tween rotateTween;

		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}

		public override void Show()
		{
			base.Show();
			rotateTween = CircleLoading.transform
				.DOLocalRotate(new Vector3(0f, 0f, rotationAmount), duration, RotateMode.LocalAxisAdd)
				.SetLoops(-1, LoopType.Restart)
				.SetEase(easeType);
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetMessageText(string value)
		{
			if (MessageText)
				MessageText.SetText(value);
		}

		public void SetCancelAction(Action value)
		{
			if (CancelButton)
				CancelButton.Subscribe(value);
		}

		public void ClearCancelActions()
		{
			if (CancelButton)
				CancelButton.UnsubscribeAll();
		}

		private void Clear()
		{
			if (CancelButton)
				CancelButton.Clear();

			rotateTween?.Kill();
			rotateTween = null;
		}
	}
}