using Berserk.Shared.GameCore.Commands;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface ICommandController
	{
		void Execute(string userId, string cmd, CmdParamsModel cmdParamsModel, bool isNested = false);
		void Execute<T>(string userId, CmdParamsModel cmdParamsModel, bool isNested = false) where T : Command;
	}
}