using System;
using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.Settings
{
	public interface IReportView
	{
		event Action<ReportReason> OnReport;
		void Show();
		void Close();
	}
}