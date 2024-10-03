using System.Linq;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Abstractions;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BerserkV3.Startup.Authorization
{
	public class AuthenticationApplication : DisposableWithCts, IAuthenticationApplication
	{
		private readonly IStateMachine stateMachine;
		private readonly IInstantiator instantiator;

		public AuthenticationApplication(
			IStateMachine stateMachine,
			IInstantiator instantiator)
		{
			this.stateMachine = stateMachine;
			this.instantiator = instantiator;
			stateMachine.SetId(GetType().Name);
		}

		public async UniTask AuthenticationAsync(params IAuthArg[] args)
		{
			var redirectId = GetRedirectId(args);
			var authProcess = new UniTaskCompletionSource();
			if (!stateMachine.Any())
			{
				stateMachine.OnSwitchState += StateSwitched;
				RegisterState<AuthIntroState>();
				RegisterState<AuthChooseState>();
				RegisterState<AuthByTokenState>();
				RegisterState<AuthVerificationState>();
				RegisterState<AuthVerifyPassState>();
				RegisterState<AuthMessageState>();
				RegisterState<AuthProcessingState>();
				RegisterState<AuthReferralState>();
				RegisterState<AuthForgotPassState>();
				RegisterState<AuthResetPassState>();
				RegisterState<AuthSignInState>();
				RegisterState<AuthSignUpState>();
				RegisterState<AuthThermsState>();
				RegisterState<AuthSocialSignInState>();
				RegisterState<AuthCompleteState>(stateMachine);
			}
			
			stateMachine.Switch(redirectId, User.GetRedirections<IAuthArg>().ToArray<object>());
			User.RemoveRedirections<IAuthArg>();
			await authProcess.Task.AttachExternalCancellation(Token);
			return;

			void StateSwitched(string stateId)
			{
				if (stateId != nameof(AuthCompleteState))
					return;

				stateMachine.OnSwitchState -= StateSwitched;
				stateMachine.Clear();
				authProcess.TrySetResult();
			}
		}

		public async UniTask<bool> AuthenticationRedirectAsync(params IAuthArg[] args)
		{
			if (!User.IsAuthorized)
				return false;
			
			if (GetRedirectId(args) == nameof(AuthCompleteState))
				return true;

			await AuthenticationAsync(args);
			return true;
		}

		private static string GetRedirectId(params IAuthArg[] args)
		{
			return args.OfType<AuthRedirectionArg>()
				.Select(x => x.StateId)
				.DefaultIfEmpty(nameof(AuthIntroState))
				.FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
		}

		private void RegisterState<TState>(params object[] args) where TState : IState
		{
			stateMachine.Add(instantiator.Instantiate<TState>(args));
		}
	}
}