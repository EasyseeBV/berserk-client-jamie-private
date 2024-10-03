using System.Collections.Generic;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore;

namespace Berserk.Shared.Data.Abstraction
{
	/// <summary>
	/// Do not use directly, only trough IRuntimePlayer controller.
	/// All properties with setters is need locate here.
	/// To make a developer to use IRuntimePlayer controller to set any value there.
	/// </summary>
	public interface IRuntimePlayerInternal
	{
		bool IsReady { get; set; }
		bool IsFinishedMulligan { get; set; }
		int? TurnsWithoutCards { get; set; }
		int LastRoundWithActive { get; set; }
	}
	
	public interface IRuntimePlayerData : IRuntimeDataBase
	{
		string UserId { get; }
		string DeckId { get; }
		string UserName { get; }
		bool IsFirstMover { get; }
		bool IsBot { get; }
		
		bool IsReady { get; }
		bool IsFinishedMulligan { get; }
		int? TurnsWithoutCards { get; }
		int LastRoundWithActive { get; }
		
		IntStat Mana  { get; }
		IntStat HandCount  { get; }
		List<RuntimePlayedCardData> PlayedCards { get; }
	}
}