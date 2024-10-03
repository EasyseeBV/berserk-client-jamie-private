using Berserk.Shared.Data.Enums;
using Game.Entities;
using RR.Core.EventLayer;
using UnityEngine;
using Vulcan.Data;
using CardData = Vulcan.Data.CardData;
using Random = System.Random;

namespace Events
{
	/// <summary>
	/// TODO remove, use instead GameCoreBus
	/// </summary>
	public class GameBus : EventBus
	{
		static GameBus()
		{
			InitFields<GameBus>();
			RoundTimer.HideInLog = true;
			OnSpawnCard.HideInLog = false;
			OnEntityTarget.HideInLog = true;
			OnLavaChanged.HideInLog = true;
			OnDamageDealt.HideInLog = true;
			OnSpawnCard.EnableChainPublishing = true;
			OnEntityDie.EnableChainPublishing = true;
		}

		public static void ResetStates()
		{
			RoundTimer.Assign(0);
			OnEntityTarget.Assign(default);
			LocalContext?.Clear();
			LocalContext = new LocalContext();

			OnReconnectRequired.Clear();
			OpponentDisconnected.Clear();
			RoundTimer.Clear();
			OnTimerPaused.Clear();
			CurrentRound.Clear();
			CurrentRound = new State<RoundData>();
			TargetSelecting.Clear();
			OnEntityTarget.Clear();
			FetchContext.Clear();
		}

		public static LocalContext LocalContext = new();

		//Session contol
		public static State<bool> OpponentDisconnected;
		public static State<bool> FetchContext;
		public static RREvent OnContextUpdated;

		//Round
		public static State<int> RoundTimer;
		public static State<bool> OnTimerPaused;
		public static State<RoundData> CurrentRound; // server
		public static RREvent OnPassingTurned; // local
		public static RREvent OnRoundEnd;

		//Network
		public static State<bool> OnReconnectRequired;

		//UI control
		public static RREvent<BlockedInfo> OnActionBlocked;
		public static RREvent<bool> OnBlockUI;
		public static State<bool> TargetSelecting;
		public static RREvent OnPickedConfirmed;
		public static RREvent OnTimerStateUpdated;

		//Table
		public static RREvent<CardData> OnSpawnCard;
		public static RREvent<DataBase> OnSpawnConfirmed;
		public static RREvent<DataBase> OnSpawnCanceled;
		public static State<IInteractiveEntity> OnEntityTarget;
		public static RREvent<Owner, int> OnLavaChanged;
		public static RREvent<Vector3, int> OnDamageDealt; // Call damage bubble
		public static RREvent<DataBase> OnEntityDie;
		public static RREvent UpdateGraveyard;
		public static RREvent<DataBase> OnVulcaniteDies;

		//Hand
		public static RREvent OnShowMulligan;
		public static RREvent<CardData[]> SpawnCardsInHand;
		public static RREvent<Owner> OnRequestHandRearrange;
	}
}