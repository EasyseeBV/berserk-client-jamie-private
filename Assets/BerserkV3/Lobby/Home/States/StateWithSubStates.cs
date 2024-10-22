using System;
using System.Collections.Generic;
using System.Linq;
using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.Home.Args;
using RR.Core.DebugSystem;
using RR.UIService;
using Zenject;

namespace BerserkV3.Lobby.Home.States
{
	public abstract class StateWithSubStates : State
	{
		protected readonly IStateMachine SubStateMachine;
		protected readonly IInstantiator Instantiator;
		
		
		protected StateWithSubStates(
			IStateMachine subStateMachine, 
			IInstantiator instantiator, 
			IState parentState = null)
			: base(parentState)
		{
			SubStateMachine = subStateMachine;
			Instantiator = instantiator;
			SubStateMachine.SetId(GetType().Name);
		}
		
		public override void OnExit()
		{
			base.OnExit();
			SubStateMachine.Switch(EmptyState.Id);
		}

		public override IEnumerable<IState> GetAllStates()
		{
			var states = SubStateMachine.GetStates().SelectMany(x => x.GetAllStates());
			
			return base.GetAllStates().Concat(states);
		}
		
		protected void RegisterState<TState>(params object[] args) where TState : IState
		{
			SubStateMachine.Add(Instantiator.Instantiate<TState>(args));
		}

		protected bool TryRedirect(params object[] args)
		{
			try
			{
				var targetArg = args.LastOrDefault(arg => arg is MainMenuSubStateRedirectionArg);
				if (targetArg is not MainMenuSubStateRedirectionArg redirectArg)
					return false;
				
				var newArgs = args.Where(x => x != targetArg).ToArray();
				OnRedirected(redirectArg.StateId, newArgs);
				return true;
			}
			catch (Exception e)
			{
				RRLogger.Error(e, "Cannot get substate redirect. Check substates.");
				throw;
			}
		}
		
		protected virtual void OnRedirected(string stateId, params object[] args)
		{
			SubStateMachine.Switch(stateId, args);
		}
	}
}