using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Common.LiveLinkRouter;
using BerserkV3.Common.StateMachine;
using UnityEngine;
using Zenject;

namespace BerserkV3.Startup.Authorization
{
	public abstract class AuthState<TArg> : State where TArg : IAuthArg
	{
		private readonly List<object> stateArgs = new();
		protected ILiveLinkRouter LinkRouter;
		protected IGameDatabase GameDatabase;
		private CancellationTokenSource lifeTime;
		protected CancellationToken Token => lifeTime?.Token ?? CancellationToken.None;
		

		[Inject]
		protected void Construct(ILiveLinkRouter linkRouter, IGameDatabase gameDatabase)
		{
			LinkRouter = linkRouter;
			GameDatabase = gameDatabase;
		}

		public sealed override void OnEnter(params object[] args)
		{
			if (!Application.isPlaying)
				return;
			
			lifeTime?.Cancel();
			lifeTime?.Dispose();
			lifeTime = new CancellationTokenSource();
			AddStateArgs(args);
			OnEnter(GetArgs());
		}
		
		public sealed override void OnExit()
		{
			if (!Application.isPlaying)
				return;
			
			base.OnExit();
			lifeTime?.Cancel();
			lifeTime?.Dispose();
			lifeTime = null;
			OnExit(GetArgs());
			stateArgs.Clear();
		}

		protected abstract void OnEnter(TArg args);
		protected virtual void OnExit(TArg args){}
		
		protected TArg GetArgs()
		{
			return stateArgs.OfType<TArg>().FirstOrDefault();
		}
		
		protected object[] GetRawArgs()
		{
			return stateArgs.ToArray();
		}

		protected object[] GetStateArgs()
		{
			var exceptType = typeof(TArg);
			return stateArgs.Where(x => x.GetType() != exceptType).ToArray();
		}

		protected void AddStateArgs(params object[] args)
		{
			stateArgs.AddRange(args);
		}

		protected void HandleButton(AuthButtonArg arg)
		{
			if (!Application.isPlaying)
				return;
			
			if (!string.IsNullOrEmpty(arg.State))
				StateMachineBus.Switch(arg.State, GetStateArgs());

			if (!string.IsNullOrEmpty(arg.Url))
			{
				if (arg.UrlAsId)
					LinkRouter.OpenLinkByKey(arg.Url);
				else
					LinkRouter.OpenLink(arg.Url);
			}

			arg.CallBack?.Invoke();
		}
	}
}