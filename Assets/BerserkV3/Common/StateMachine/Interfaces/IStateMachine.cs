using System;
using System.Collections.Generic;

namespace BerserkV3.Common.StateMachine
{
    public interface IStateMachine
    {
        event Action<string> OnSwitchState;
        
        string Id { get; }
        int Count { get; }
        
        IState Current { get; }
        
        IState Previous { get; }

        bool Contains(string id);
        
        void SetId(string id);
        
        void Add(IState state);
        
        void Remove(string stateId);
        
        void Switch(string stateId, params object[] args);

        IEnumerable<IState> Get();

        bool Any();
        
        void Clear();
    }
}