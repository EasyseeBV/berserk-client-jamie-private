using System.Collections.Generic;

namespace Berserk.Shared.GameCore
{
	public interface IStatModifiable<T> : IStat<T>
	{
		T TotalMax { get; }
		T ModMax { get;}
		
		T TotalCurrent { get; }
		T ModCurrent { get;}
		
		void AddModMax(T value);
		void SetModMax(T value);
		
		void AddModCurrent(T value);
		void SetModCurrent(T value);

		
		void CalculateModifiers(bool notify = true);
		IEnumerable<IStatModifier<T>> GetModifiers();
		IEnumerable<IStatModifier<T>> GetModifiers(string id);
		void AddModifier(IStatModifier<T> value, bool notify = true);
		void RemoveModifier(IStatModifier<T> value, bool notify = true);
		void RemoveModifiers(string id, bool notify = true);
		void ClearModifiers(bool notify = true);
	}
}