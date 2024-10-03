using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.Network;
using BerserkV3.GameCore.Repository;

namespace BerserkV3.GameCore.Settings
{
	public class ReportService : IReportService
	{
		private readonly IGameContext gameContext;
		private readonly IGameRepository gameRepository;
		private bool reportIsSent;

		public bool IsAvailable => !reportIsSent && gameContext.PlayerRepository.GetOpposite(gameRepository?.SelfId)?.RuntimeData?.IsBot == false;

		public ReportService(IGameContext gameContext, IGameRepository gameRepository)
		{
			this.gameContext = gameContext;
			this.gameRepository = gameRepository;
		}

		public async Task SendReportAsync(ReportReason reportReason)
		{
			var resut = await GameAPI.PostReportOpponent(reportReason);
			reportIsSent = resut == System.Net.HttpStatusCode.OK;
		}
	}
}