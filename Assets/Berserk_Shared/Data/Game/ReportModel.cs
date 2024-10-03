using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game
{
	public class ReportModel
	{
		public ReportReason ReportReason { get; set; }

		public ReportModel(ReportReason reportReason)
		{
			ReportReason = reportReason;
		}
	}
}