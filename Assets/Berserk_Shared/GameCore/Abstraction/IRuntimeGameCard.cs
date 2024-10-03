using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeGameCard : IRuntimeGameObject
	{
		new IRuntimeCardData RuntimeData { get; }
		new ICardData Data { get; }

		void TurnToken(bool value);
		
		void ReturnToDiscard(bool notify = true);

		void ReturnToDeck(bool notify = true);

		void ReturnToHand(bool notify = true);

		void ReturnToTable(bool notify = true);

		void ReturnToChoose(bool notify = true);
		
		void ReturnToShowAll(bool notify = true);
		
		void ReturnToShow(bool notify = true);

		void ReturnToExile(bool notify = true);
		
		void UpdateState(RuntimeState value, bool notify = true);
		void UpdateState(RuntimeState current, RuntimeState? prev, bool notify = true);
	}
}