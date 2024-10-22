using System;

namespace Berserk.Shared.GameCore.LogicContext
{
	public interface ISharedLogger
	{
		void Log(string message);
		void Error(string message);
		void Error(Exception exception);
		void Error(Exception exception, string message);
	}
	
	public static class DefaultSharedLogger
	{
		private static ISharedLogger logger;

		public static void Initialize(ISharedLogger logger)
		{
			DefaultSharedLogger.logger = logger;
		}

		public static void Log(string message)
		{
			logger.Log(message);
		}

		public static void Error(string message)
		{
			logger.Error(message);
		}

		public static void Error(Exception exception, string message)
		{
			logger.Error(exception, message);
		}

		public static void Error(Exception exception)
		{
			logger.Error(exception);
		}
	}
}