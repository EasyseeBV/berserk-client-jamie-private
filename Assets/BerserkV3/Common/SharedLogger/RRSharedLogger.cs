using System;
using Berserk.Shared.GameCore.LogicContext;
using RR.Core.DebugSystem;

namespace BerserkV3.Generic.SharedLogger
{
	public class RRSharedLogger : ISharedLogger
	{
		public RRSharedLogger()
		{
			DefaultSharedLogger.Initialize(this);
		}
		public void Log(string message)
		{
			RRLogger.Log(message);
		}

		public void Error(string message)
		{
			RRLogger.Error(message);
		}

		public void Error(Exception exception)
		{
			RRLogger.Error(exception);
		}
	}
}