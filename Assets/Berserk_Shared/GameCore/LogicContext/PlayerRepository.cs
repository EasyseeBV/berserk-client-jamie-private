using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class PlayerRepository : List<IRuntimePlayer>, IPlayerRepository
	{
		public PlayerRepository() : base(){}
		public PlayerRepository(IEnumerable<IRuntimePlayer> players) : base(players){}
		
		public IRuntimePlayer[] GetAll()
		{
			return ToArray();
		}

		public IRuntimePlayer Get(string userId)
		{
			return this.FirstOrDefault(p => userId == p?.RuntimeData?.UserId);
		}
		
		public IRuntimePlayer Get(Func<IRuntimePlayer, bool> predicate)
		{
			return predicate != null
				? this.FirstOrDefault(predicate.Invoke)
				: default;
		}
		
		public IRuntimePlayer GetOpposite(string userId)
		{
			return !string.IsNullOrEmpty(userId)
				? this.FirstOrDefault(p => userId != p?.RuntimeData?.UserId)
				: default;
		}

		public bool TryGet(Func<IRuntimePlayer, bool> predicate, out IRuntimePlayer result)
		{
			result = Get(predicate);
			return result != null;
		}

		public bool TryGet(string userId, out IRuntimePlayer result)
		{
			result = Get(userId);
			return result != null;
		}

		public string[] GetIds()
		{
			return this
				.Select(p => p.UserId)
				.ToArray();
		}

		public void Sync(params IRuntimePlayerData[] runtimeDatas)
		{
			if (runtimeDatas == null || runtimeDatas.Length == 0)
				throw new ArgumentException($"Any {nameof(IRuntimePlayerData)}s found.");
			
			if (Count == 0)
				throw new NullReferenceException($"Any {nameof(IRuntimePlayer)} found.");
			
			if (Count < runtimeDatas.Length)
				throw new InvalidOperationException($"Not enough {nameof(IRuntimePlayer)}s , Exist: {Count}, Requested: {runtimeDatas.Length}");
			
			foreach (var runtimeData in runtimeDatas)
			{
				if (!TryGet(runtimeData.UserId, out var player))
					player = Get(p => p.RuntimeData == null);
				
				if (player == null)
					throw new NullReferenceException($"Any free {nameof(IRuntimePlayer)} found.");
				
				player.Sync(runtimeData);
			}
		}

		public new void Clear()
		{
			ForEach(p => p?.Dispose());
			base.Clear();
		}
	}
}