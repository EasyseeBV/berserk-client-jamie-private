using System;
using DG.Tweening;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public partial class AuthSocialResponseView : SafeView
	{
		[SerializeField] private float rotationAmount = -360f;
		[SerializeField] private float duration = 2f;
		[SerializeField] private Ease easeType = Ease.Linear;
		
		private Tween rotateTween;
		
		protected override void OnShown()
		{
			base.OnShown();
			rotateTween = CircleLoading.transform.DOLocalRotate(new Vector3(0f, 0f, rotationAmount), duration, RotateMode.LocalAxisAdd)
				.SetLoops(-1, LoopType.Restart)
				.SetEase(easeType);
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}
		
		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}
		
		private void OnDestroy()
		{
			Clear();
		}
		
		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}
		
		public void SetMessageText(string value)
		{
			Set(MessageText, value);
		}
		
		public void SetCancelAction(Action value)
		{
			CancelButton.Subscribe(value);
		}

		public void ClearCancelActions()
		{
			CancelButton.UnsubscribeAll();
		}

		private void Clear()
		{
			CancelButton.Clear();
			rotateTween?.Kill();
			rotateTween = null;
		}
	}
}