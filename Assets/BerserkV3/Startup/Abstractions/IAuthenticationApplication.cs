using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Abstractions
{
	public interface IAuthenticationApplication
	{
		UniTask AuthenticationAsync(params IAuthArg[] args);
		UniTask<bool> AuthenticationRedirectAsync(params IAuthArg[] args);
	}
}