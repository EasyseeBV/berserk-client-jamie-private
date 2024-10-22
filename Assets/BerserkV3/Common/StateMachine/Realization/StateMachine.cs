using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.Common.StateMachine
{
    public class StateMachine : IStateMachine, IDisposable
    {
        public event Action<string> OnSwitchState;

        public int Count => stateStorage.Count - 1;
        public IState Current { get; private set; }
        public IState Previous { get; private set; }
        public object[] PreviousArgs { get; private set; }
        public object[] CurrentArgs { get; private set; }
        
        public string Id { get; private set; }
        
        private readonly Dictionary<string, IState> stateStorage;

        public StateMachine()
        {
	        stateStorage = new Dictionary<string, IState>();
            Clear();
            StateMachineBus.OnSwitchRequested += OnHubSwitchRequested;
            Id ??= Guid.NewGuid().ToString();
        }

        public StateMachine(string id) : this()
        {
	        Id = id;
        }

        public bool Contains(string id)
        {
            return !string.IsNullOrEmpty(id) && stateStorage.ContainsKey(id);
        }

        public void SetId(string id)
        {
	        Id = id;
        }

        public void Add(IState state)
        {
	        if (!Application.isPlaying)
		        return;
	        
            if (string.IsNullOrEmpty(state?.Id))
	            throw new ArgumentException("State is Null");

            if (stateStorage.ContainsKey(state.Id))
	            throw new ArgumentException($"State already exist  {state.Id}");

            stateStorage.Add(state.Id, state);
            state.Order = stateStorage.Count-2;
        }

        public void Remove(string stateId)
        {
	        if (!Application.isPlaying)
		        return;
	        
            if (string.IsNullOrEmpty(stateId))
	            throw new ArgumentException("StateId is Null");

            if (stateId == EmptyState.Id)
	            return;

            if (Previous != null && Previous.Id == stateId)
	            Previous = stateStorage.GetValueOrDefault(EmptyState.Id);

            if (Current != null && Current.Id == stateId)
	            Switch(EmptyState.Id);

            if (!stateStorage.TryGetValue(stateId, out var state))
	            return;

            if (state != null)
				state.Order = 0;
            
	        stateStorage.Remove(stateId);
        }

        public void Switch(string stateId, params object[] args)
        {
	        if (!Application.isPlaying)
		        return;
	        
            if (string.IsNullOrEmpty(stateId) || !stateStorage.ContainsKey(stateId))
	            throw new ArgumentException($"State does not exist : {stateId}");

            Current?.OnExit();
            PreviousArgs = CurrentArgs ?? Array.Empty<object>();
            Previous = Current ?? stateStorage[EmptyState.Id];
            Current = stateStorage[stateId];
            CurrentArgs = args ?? Array.Empty<object>();
            Current.OnEnter(args);
            OnSwitchState?.Invoke(stateId);
        }

        public IEnumerable<IState> GetStates(bool includeDefault = false)
        {
	        return includeDefault 
		        ? stateStorage.Values.OrderBy(x => x.Order).ToArray() 
		        : stateStorage.Values.Where(x=> x.Id != EmptyState.Id).OrderBy(x => x.Order).ToArray();
        }

        public IState GetState(string stateId)
        {
	        if (string.IsNullOrEmpty(stateId))
		        return default;
	        
	        return stateStorage.TryGetValue(stateId, out var state) ? state : default;
        }

        public IState GetState(int index)
        {
	        return stateStorage.Values.FirstOrDefault(x => x.Order == index);
        }

        public bool Any()
        {
            return Count > 1;
        }

        public void Clear()
        {
            stateStorage.Clear();
            PreviousArgs = CurrentArgs = Array.Empty<object>();
            Current = Previous = new EmptyState();
            Add(Current);
        }

        public void Dispose()
        {
            PreviousArgs = CurrentArgs = null;
            Current = Previous = null;
            stateStorage.Clear();
            StateMachineBus.OnSwitchRequested -= OnHubSwitchRequested;
        }

        private void OnHubSwitchRequested(StateMachineArgs args)
        {
	        RRLogger.Log($"{ToString()}, \nNextState : {args.StateId}");
            if (!Contains(args.StateId)) 
                return;

            var stateId = args.Previous 
                ? Previous.Id 
                : args.StateId;
            
            var objectArgs = args.Previous 
                ? args.Parameters ?? PreviousArgs 
                : args.Parameters ?? Array.Empty<object>();
            
            Switch(stateId, objectArgs);
        }

        public override string ToString()
        {
	        var stateMachineName = GetType().Name + " : " + Id;
	        return $"[{stateMachineName.Orange()}]\nCurrentState : {Current.Id}, \nPreviousState : {Previous.Id}";
        }
    }
}