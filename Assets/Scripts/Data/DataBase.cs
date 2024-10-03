using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using RR.Core.Extensions;
using Sirenix.Utilities;

namespace Vulcan.Data
{
	[Serializable]
	public abstract class DataBase : IEquatable<DataBase>
	{
		public string UID;

		protected DataBase()
		{
			Id = Id ?? Guid.NewGuid().ToString();
		}

		public EffectsContainer EffectsContainer = new EffectsContainer();

		public string Id;
		public Owner Owner;

		public CardStat Attack;
		public CardStat Hp;
		public CardStat Lava;

		public string Title;
		public string ArtUrl;

		public bool IsSpawnedByEffect;

		// the table card does not need to be synchronized, because it came from the signal
		public bool IsSpawnedByResolver;

		public bool Equals(DataBase other)
		{
			if (ReferenceEquals(null, other))
				return false;

			return string.IsNullOrEmpty(UID) && string.IsNullOrEmpty(other.UID)
				? Id.Same(other.Id) //deck builder
				: UID.Same(other.UID); //game session
		}

		public override string ToString()
		{
			return string.Join("\n", GetType().GetFields().Select(x => $"[{x.Name} : {x.GetValue(this)}]"));
		}
	}
}