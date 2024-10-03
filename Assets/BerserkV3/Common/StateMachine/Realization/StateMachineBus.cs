using System;
using UnityEngine;

namespace BerserkV3.Common.StateMachine
{
    public class StateMachineBus
    {
        public static event Action<StateMachineArgs> OnSwitchRequested;

        private static void Publish(StateMachineArgs args)
        {
	        if (!Application.isPlaying)
		        return;
	        
            OnSwitchRequested?.Invoke(args);
        }

        public static void Switch(string stateId)
        {
            Publish(new StateMachineArgs {StateId = stateId});
        }

        public static void Switch(string stateId, params object[] args)
        {
            Publish(new StateMachineArgs {StateId = stateId, Parameters = args});
        }

        public static void Switch<TState>(params object[] args) where TState : IState
        {
            Publish(new StateMachineArgs {StateId = typeof(TState).Name, Parameters = args});
        }

        public static void Switch<TState>() where TState : IState
        {
            Publish(new StateMachineArgs {StateId = typeof(TState).Name});
        }

        public static void Previous(string stateId)
        {
            Publish(new StateMachineArgs {StateId = stateId, Previous = true});
        }

        public static void Previous(string stateId, params object[] args)
        {
            Publish(new StateMachineArgs {StateId = stateId, Previous = true, Parameters = args});
        }
    }
}