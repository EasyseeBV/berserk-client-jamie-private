using System;
using NUnit.Framework;
using RR.Core.DebugSystem;

public class RRLoggerTest
{
	[Test]
	public void LogToConsole()
	{
		void ExceptionMethod()
		{
			throw new Exception("My super exception", new ArgumentNullException("vasya", "vasya is null"));
		}

		Assert.Throws<Exception>(ExceptionMethod);

		try
		{
			ExceptionMethod();
		}
		catch (Exception e)
		{
			Assert.DoesNotThrow(() => RRLogger.Error(e));
			Assert.Pass();
		}
	}
}
