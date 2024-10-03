using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.LogicContext;

namespace BerserkV3.Generic.UndoSystem
{
	public class UndoSystem : IUndoSystem, IDisposable
	{
		private readonly Dictionary<string, List<Action>> undoStorage;
		private readonly List<string> orders;

		public UndoSystem()
		{
			undoStorage = new Dictionary<string, List<Action>>();
			orders = new List<string>();
		}
		
		public void Add(string id, Action undoAction)
		{
			if (AssertEmptyId(id))
				return;
			
			if (!Contains(id))
			{
				undoStorage.Add(id, new List<Action>());
				orders.Add(id);
			}
			
			undoStorage[id].Add(undoAction);
		}

		public void Add(Action undoAction)
		{
			var uniqUndoId = Guid.NewGuid().ToString();
			Add(uniqUndoId, undoAction);
		}

		public void Remove(string id)
		{
			if (!Contains(id))
				return;
			
			undoStorage[id]?.Clear();
			undoStorage.Remove(id);
			orders.Remove(id);
		}

		public void RemoveLast()
		{
			Remove(orders.LastOrDefault());
		}

		public bool Contains(string id)
		{
			return !AssertEmptyId(id) && orders.Contains(id);
		}

		public void Undo(string id)
		{
			if (!Contains(id))
				return;
			
			undoStorage[id]?.ForEach(a => a?.Invoke());
			Remove(id);
		}

		public void Undo()
		{
			var id = orders.LastOrDefault();
			Undo(id);
		}

		public void Clear()
		{
			undoStorage.Clear();
			orders.Clear();
		}

		public void Dispose()
		{
			Clear();
		}

		private bool AssertEmptyId(string id)
		{
			if (!string.IsNullOrEmpty(id)) 
				return false;
			
			DefaultSharedLogger.Error($"{GetType().Name} : Id is null or empty");
			return true;
		}
	}
}