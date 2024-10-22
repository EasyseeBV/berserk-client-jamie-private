namespace Berserk.Shared.GameCore
{
	public interface IStatModifier<T>
	{
		string Id { get; }
		int Priority { get; }

		T ModifierCurrent { get; }
		T ModifierMaximum { get; }
		T MaximumApplied { get; }
		T CurrentApplied { get; }

		void Apply(IStatModifiable<T> stat);
		void Expire(IStatModifiable<T> stat);
		
		IStatModifier<T> SetModifierId(string id);
		IStatModifier<T> SetMaxModifier(T value, T applied = default, bool revertWhenExpire = true);
		IStatModifier<T> SetCurrModifier(T value, T applied = default, bool revertWhenExpire = true);
		ModifierStack<T> GetDataStack(ModifierStack<T> other = default);
		IStatModifier<T> SetPriority(int value);
	}
}