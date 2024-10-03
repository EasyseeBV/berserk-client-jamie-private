using System;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Network;
using ServerCore.Infrastructure.Models;
using Vulcan.Network;
using Vulcan.Network.Context;

namespace Vulcan.Report.Application
{
	[Obsolete]
	public class ReportService
	{
		private bool reportIsSent;
		private Action<bool> callback;

		public bool IsAvailable => !reportIsSent && !ActorsContextResolver.IsPracticeMode;

		public void Subscribe(Action<bool> result)
		{
			callback = result;
		}

		public async Task SendReportAsync(ReportReason reportReason)
		{
			var resut = await GameAPI.PostReportOpponent(reportReason);
			reportIsSent = resut == System.Net.HttpStatusCode.OK;
			callback.Invoke(reportIsSent);
		}
	}
}