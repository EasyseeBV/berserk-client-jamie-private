using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.Abstractions
{
	public interface IRedirectionApplication
	{
		UniTask<bool> RedirectAsync();
	}
}