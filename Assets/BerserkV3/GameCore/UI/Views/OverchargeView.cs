using System;
using BestHTTP.Extensions;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;

namespace BerserkV3.GameCore.UI
{
	public interface IOverchargeView
	{
		void SetConfirmAction(Func<UniTask> action);
		void SetIncreaseAction(Action action);
		void SetDecreaseAction(Action action);

		void SetInteractable(bool value);
		void SetHeaderText(string value);
		void SetValueText(string value);
		void Show();
		void Close();
		void Clear();
	}

	public partial class OverchargeView : BaseView, IOverchargeView
	{
		public void SetConfirmAction(Func<UniTask> action)
		{
			if (!BackBtn)
				return;
			
			BackBtn.onClick.RemoveAllListeners();
			BackBtn.onClick.AddListener(() => action?.Invoke());
		}

		public void SetIncreaseAction(Action action)
		{
			if (!IncreaseValueButton)
				return;
			
			IncreaseValueButton.onClick.RemoveAllListeners();
			IncreaseValueButton.onClick.AddListener(() => action?.Invoke());
		}

		public void SetDecreaseAction(Action action)
		{
			if (!DecreaseValueButton)
				return;
			
			DecreaseValueButton.onClick.RemoveAllListeners();
			DecreaseValueButton.onClick.AddListener(() => action?.Invoke());
		}

		public void SetInteractable(bool value)
		{
			CanvasGroup.interactable = value;
		}

		public void SetHeaderText(string value)
		{
			if (!HeaderText)
				return;

			SetActive(HeaderText, !string.IsNullOrEmpty(value));
			Set(HeaderText, value);
		}

		public void SetValueText(string value)
		{
			SelectedCount.SetText(value);
		}

		public void Clear()
		{
			if (IncreaseValueButton)
				IncreaseValueButton.onClick.RemoveAllListeners();
			
			if (DecreaseValueButton)
				DecreaseValueButton.onClick.RemoveAllListeners();
			
			if (BackBtn)
				BackBtn.onClick.RemoveAllListeners();
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
	}
}