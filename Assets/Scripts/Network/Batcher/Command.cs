using System;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace Vulcan.Network
{
	public class Command
	{
		private readonly Action action;
		public CommandType CommandType { get; }

		public Command(Action action, CommandType commandType)
		{
			this.action = action;
			CommandType = commandType;
		}

		public void Perform()
		{
			action?.Invoke();
			RRLogger.Log($"[{"Command".Yellow().Bold()}]: {nameof(Perform)} {CommandType} Action - {action?.Method.Name}");
		}
	}
}