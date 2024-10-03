using System;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Abstractions
{
	public interface IFirstVulcaniteApplication : IDisposable
	{
		UniTask<bool> SelectFirstVulcanteAsync();
	}
}