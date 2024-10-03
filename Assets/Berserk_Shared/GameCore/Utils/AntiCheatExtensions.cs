using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.Utils
{

	public static class AntiCheatExtensions
	{
		/// <summary>
		/// The string is different for all user ids, it can't be send trough queue controller because the User with that id doesn't exist.
		/// </summary>
		private const string NO_ONE_ACCESS = "Any user will not receive this state, because access level is : AccessLevel.NoOne";
		
		/// <summary>
		/// Check if user сomplies with anti-cheat rules
		/// </summary>
		/// <param name="receiverId">Object owner filtered by AntiCheat</param>
		/// <param name="userId">Target user to check</param>
		/// <returns></returns>
		public static bool IsAccessibleReceiver(this string receiverId, string userId)
		{
			// If the value is null, it can be shown to everyone.
			// Or userId, which can receive this data.
			// Otherwise, the message cannot be received by any user.
			return receiverId == null || receiverId == userId;
		}
		
		/// <summary>
		/// Get [UserId] who can receive a data.
		/// </summary>
		/// <param name="value">Target to determine which user can receive a data</param>
		/// <returns>If the value is [Null] it can be shown to everyone. If the value [UserId] it can receive a data. Otherwise the access blocked for any user.</returns>
		public static string GetAccessibleReceiver(this IRuntimeGameObject value)
		{
			return GetAccessibleReceiver(GetAccessLevel(value), value?.RuntimeData?.OwnerUserId);
		}

		/// <summary>
		/// Get [UserId] who can receive a data.
		/// </summary>
		/// <param name="value">Target to determine which user can receive a data</param>
		/// <returns>If the value is [Null] it can be shown to everyone. If the value [UserId] it can receive a data. Otherwise the access blocked for any user.</returns>
		public static string GetAccessibleReceiver(this IRuntimeData value)
		{
			return GetAccessibleReceiver(GetAccessLevel(value), value?.OwnerUserId);
		}
		
		/// <summary>
		/// Get [UserId] who can receive a data.
		/// </summary>
		/// <param name="value">Target to determine which user can receive a data</param>
		/// <returns>If the value is [Null] it can be shown to everyone. If the value [UserId] it can receive a data. Otherwise the access blocked for any user.</returns>
		public static string GetAccessibleReceiver(this IRuntimeEffect value)
		{
			return GetAccessibleReceiver(value?.RuntimeData);
		}
		
		/// <summary>
		/// Get [UserId] who can receive a data.
		/// </summary>
		/// <param name="value">Target to determine which user can receive a data</param>
		/// <returns>If the value is [Null] it can be shown to everyone. If the value [UserId] it can receive a data. Otherwise the access blocked for any user.</returns>
		public static string GetAccessibleReceiver(this IRuntimeEffectData value)
		{
			return GetAccessibleReceiver(value?.AccessLevel ?? AccessLevel.NoOne, value?.ExecutorOwnerId);
		}

		/// <summary>
		/// Get [UserId] who can receive a data.
		/// </summary>
		/// <param name="value">Target access level mask to determine which user can receive a data</param>
		/// <param name="requestedUserId">UserId for who requested the access</param>
		/// <returns>If the value is [Null] it can be shown to everyone. If the value [UserId] it can receive a data. Otherwise the access blocked for any user.</returns>
		/// <exception cref="NotImplementedException">When added new entries of <see cref="AccessLevel"/> but not extended the method.</exception>
		public static string GetAccessibleReceiver(this AccessLevel value, string requestedUserId)
		{
			// There are flags but first chose a low level one.
			if (value.HasFlag(AccessLevel.NoOne))
				return NO_ONE_ACCESS;

			if (value.HasFlag(AccessLevel.Self))
				return requestedUserId;

			if (value.HasFlag(AccessLevel.All))
				return null;

			throw new NotImplementedException($"[{nameof(GetAccessibleReceiver)}] Unknown {nameof(AccessLevel)}: {value}");
		}
		
		/// <summary>
		/// Determine the AccessLevel
		/// </summary>
		/// <returns>The AccessLevel for this entity.</returns>
		public static AccessLevel GetAccessLevel(this IRuntimeGameObject value)
		{
			return GetAccessLevel(value?.RuntimeData);
		}
		
		/// <summary>
		/// Determine the AccessLevel
		/// </summary>
		/// <returns>The AccessLevel for this entity.</returns>
		/// <exception cref="NotImplementedException">When added an new implementations of <see cref="IRuntimeData"/> but not extended the method.</exception>
		public static AccessLevel GetAccessLevel(this IRuntimeData value)
		{
			if (value == null)
				return AccessLevel.NoOne;
			
			if (value.Type == ObjectType.Hero)
				return AccessLevel.All; // allow broadcast to all of heroes, because the heroes shared to all players.

			if (value is IRuntimeCardData gameCard)
				return GetAccessLevel(gameCard.State);
			
			throw new NotImplementedException($"[{nameof(GetAccessLevel)}] Unknown {nameof(IRuntimeData)} : {value}");
		}
		
		/// <summary>
		/// Determine the AccessLevel
		/// </summary>
		/// <returns>The AccessLevel for this entity.</returns>
		/// <exception cref="NotImplementedException">When added new entries of <see cref="RuntimeState"/> but not extend the method.</exception>
		public static AccessLevel GetAccessLevel(this RuntimeState state)
		{
			return state switch
			{
				RuntimeState.InDeck => AccessLevel.NoOne,
				RuntimeState.InExile => AccessLevel.NoOne,
				RuntimeState.InHand => AccessLevel.Self,
				RuntimeState.InChoose => AccessLevel.Self,
				RuntimeState.InTable => AccessLevel.All,
				RuntimeState.InDiscard => AccessLevel.All,
				RuntimeState.InShowAll => AccessLevel.All,
				RuntimeState.InShow => AccessLevel.Self,
				_ => throw new NotImplementedException($"[{nameof(GetAccessLevel)}] Unknown {nameof(RuntimeState)}: {state}")
			};
		}
	}

}