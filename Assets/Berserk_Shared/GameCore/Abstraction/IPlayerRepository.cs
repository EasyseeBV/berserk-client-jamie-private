using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IPlayerRepository : IList<IRuntimePlayer>
	{
		IRuntimePlayer Get(string userId);
		IRuntimePlayer Get(Func<IRuntimePlayer, bool> predicate);
		IRuntimePlayer GetOpposite(string userId);
		bool TryGet(Func<IRuntimePlayer, bool> predicate, out IRuntimePlayer result);
		bool TryGet(string userId, out IRuntimePlayer result);
		string[] GetIds();
		void Sync(params IRuntimePlayerData[] runtimeDatas);
	}
}