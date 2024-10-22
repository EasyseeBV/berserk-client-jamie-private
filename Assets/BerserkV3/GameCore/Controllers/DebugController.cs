using System;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Commands.Cmd.DebugCmd;
using Berserk.Shared.GameCore.Models;
using BerserkV3.GameCore.Network.Abstraction;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UI.DebugSystem;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class DebugController : IInitializable, IDisposable
	{
		private readonly IGameHub gameHub;
		private readonly IRuntimeTimer runtimeTimer;

		public DebugController(
			IGameHub gameHub,
			IRuntimeTimer runtimeTimer)
		{
			this.gameHub = gameHub;
			this.runtimeTimer = runtimeTimer;
		}

		public void Initialize()
		{
			RRConsole.AddCommand(nameof(AddCard), AddCard, "Add card to self or opponent");
			RRConsole.AddCommand(nameof(PlayCard), PlayCard, "Play card to self or opponent");
			RRConsole.AddCommand(nameof(AddMana), AddMana, "Add manna to self");
			RRConsole.AddCommand(nameof(FullHp), FullHp, "Restore all Hp self");
			RRConsole.AddCommand(nameof(Timer), Timer, "Set timer pause/unpause");
			RRConsole.AddCommand(nameof(Manna), Manna, "Remove 100% mana enable/disable o/s");
			RRConsole.AddCommand(nameof(ClearTable), ClearTable, "Clear whole table o/s/n");
			RRConsole.AddCommand(nameof(SilenceTable), SilenceTable, "Silence whole table o/s/n");
		}
		
		public void Dispose()
		{
			RRConsole.RemoveCommands();
		}
		
		private string GetDefaultRemoteResponce()
		{
			return "This command is remote, the result will not be displayed in the console.".Orange().Bold();
		}

		private string ClearTable(string[] args)
		{
			gameHub.PerformCommandAsync<DiscardTableCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, args.JoinToString(""))).Forget();
			return GetDefaultRemoteResponce();
		}

		private string SilenceTable(string[] args)
		{
			gameHub.PerformCommandAsync<SilenceTableCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, args.JoinToString(""))).Forget();
			return GetDefaultRemoteResponce();
		}

		private string AddCard(string[] args)
		{
			if (args == null || args.Length == 0)
				return "Need parameter : ".Red() +"CardId Integer.";
			
			if (args.Contains("o"))
			{
				gameHub.PerformCommandAsync<AddCardCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, args.JoinToString(""))).Forget();
				return GetDefaultRemoteResponce();
			}
			else
			{
				foreach (var arg in args)
				{
					if(!int.TryParse(arg, out var value))
						continue;
				
					gameHub.PerformCommandAsync<AddCardCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, value.ToString())).Forget();
					return GetDefaultRemoteResponce();
				}
			}
			
			return "Need parameter : ".Red() +"CardId Integer.";
		}

		private string PlayCard(string[] args)
		{
			if (args == null || args.Length == 0)
				return "Need parameter : ".Red() +"CardId Integer.";

			gameHub.PerformCommandAsync<PlayCardDebugCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, args.JoinToString(""))).Forget();
			return GetDefaultRemoteResponce();
		}

		private string AddMana(string[] args)
		{
			if (args == null || args.Length == 0)
				return "Need parameter : ".Red() +"MannaCount Integer.";

			foreach (var arg in args)
			{
				if(!int.TryParse(arg, out var value))
					continue;
				
				gameHub.PerformCommandAsync<AddManaCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, arg)).Forget();
				return GetDefaultRemoteResponce();
			}

			return "Need parameter : ".Red() +"MannaCount Integer.";
		}

		private string FullHp(string[] args)
		{
			gameHub.PerformCommandAsync<FullHpCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash)).Forget();
			return GetDefaultRemoteResponce();
		}

		private string Timer(string[] args)
		{
			if (args == null || args.Length == 0)
				return "Need parameter one of the following is required : ".Red() + "pause, stop, break, resume, play, unpause";
			
			gameHub.PerformCommandAsync<TimerCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, args)).Forget();
			return GetDefaultRemoteResponce();
		}

		private string Manna(string[] args)
		{
			if (args == null || args.Length == 0)
				return "Need parameter one of the following is required : ".Red() + "disable, pause, stop, enable, unpause, play";
			
			gameHub.PerformCommandAsync<MannaCmd>(new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, args)).Forget();
			return GetDefaultRemoteResponce();
		}
	}
}