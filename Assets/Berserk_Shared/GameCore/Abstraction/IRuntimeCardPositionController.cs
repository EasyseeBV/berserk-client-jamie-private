namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeCardPositionController
	{
		void Process(IRuntimeGameCard target);

		/// <summary>
		/// Recalculating the position of cards in new state and in previous state
		/// </summary>
		/// <param name="changed">Target card changed</param>
		/// <param name="notify">Notify changes?</param>
		void RecalculatePositions(IRuntimeGameCard changed, bool notify = true);
	}
}