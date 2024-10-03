namespace Berserk.Shared.GameCore
{
	public struct ModifierStack<T>
	{
		public T CurrentAppliedTotal;
		public T MaximumAppliedTotal;
		public T ModifierCurrentTotal;
		public T ModifierMaximumTotal;
	}
}