using Events;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using ServerCore.Infrastructure.Models;
using Sirenix.Utilities;
using System;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.Utils;
using Vulcan.Report.Application;
using static TMPro.TMP_Dropdown;

namespace UI
{
	public partial class ReportOpponentView : BaseView
	{
		private ReportService reportService;

		protected override void OnAwake()
		{
			ReasonDropdown.options.Clear();
			foreach (ReportReason reason in Enum.GetValues(typeof(ReportReason)))
			{
				var reasonText = reason.ToString().SplitPascalCase();
				var option = new OptionData() { text = reasonText };
				ReasonDropdown.options.Add(option);
			}

			ReportBtn.Subscribe(() => SendReport((ReportReason)ReasonDropdown.value));
			BackBtn.Subscribe(Close);

			ReasonDropdown.onValueChanged.AddListener((index) => ReportBtn.interactable = index > 0);
		}

		public void SetUp(ReportService reportService)
		{
			this.reportService = reportService;
		}

		private async void SendReport(ReportReason reportReason)
		{
			await reportService.SendReportAsync(reportReason).AddLoadingTask();
			Close();
		}
	}
}