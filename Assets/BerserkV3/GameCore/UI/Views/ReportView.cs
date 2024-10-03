using System;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Settings;
using RR.UI.FrameSystem;
using ServerCore.Infrastructure.Models;
using Sirenix.Utilities;
using static TMPro.TMP_Dropdown;

namespace BerserkV3.GameCore.UI
{
	public partial class ReportView : BaseView, IReportView
	{
		public event Action<ReportReason> OnReport;

		protected override void OnAwake()
		{
			ReasonDropdown.options.Clear();
			foreach (ReportReason reason in Enum.GetValues(typeof(ReportReason)))
			{
				var reasonText = reason.ToString().SplitPascalCase();
				var option = new OptionData { text = reasonText };
				ReasonDropdown.options.Add(option);
			}

			Subscribe(ReportBtn, () => OnReport?.Invoke((ReportReason)ReasonDropdown.value));
			Subscribe(BackBtn, Close);
			ReasonDropdown.onValueChanged.AddListener(index => SetInteractable(ReportBtn, index > 0));
		}

		private void OnDestroy()
		{
			ReasonDropdown.options.Clear();
			ReportBtn.onClick.RemoveAllListeners();
			BackBtn.onClick.RemoveAllListeners();
			ReasonDropdown.onValueChanged.RemoveAllListeners();
			OnReport = null;
		}
	}
}