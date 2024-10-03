using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimePlayer : IDisposable
	{
		event Action OnLavaChanged;
		event Action OnDataChanged;
		string UserId => RuntimeData?.UserId;
		IRuntimePlayerData RuntimeData { get; }
		
		void Sync(IRuntimePlayerData runtimeData);
		void SpendLava(int lava, bool notify = true);
		void SetReady(bool value, bool notify = true);
		void SetFinishedMulligan(bool value, bool notify = true);
		void SetLastRoundActive(int value, bool notify = true);
		void AddTurnWitoutCards(int value, bool notify = true);
		void AddPlayedCard(RuntimePlayedCardData value, bool notify = true);
		void RemovePlayerCardAt(int index, bool notify = true);
	}
}