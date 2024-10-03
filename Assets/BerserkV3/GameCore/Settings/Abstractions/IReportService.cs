using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.Settings
{
	public interface IReportService
	{
		bool IsAvailable { get; }
		Task SendReportAsync(ReportReason reportReason);
	}
}