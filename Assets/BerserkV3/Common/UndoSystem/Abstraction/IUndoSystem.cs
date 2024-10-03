using System;

namespace BerserkV3.Generic.UndoSystem
{
	public interface IUndoSystem
	{
		void Add(string id, Action undoAction);
		void Add(Action undoAction);
		void Remove(string id);
		
		void RemoveLast();
		bool Contains(string id);
		void Undo(string id);
		void Undo();
		void Clear();
	}
}