using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.DataBase;
using Events;
using Game.Effect_System;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Vulcan.Data;
using Vulcan.Network;
using CardData = Vulcan.Data.CardData;
using EffectPhase = Vulcan.Data.EffectPhase;

namespace Game.Entities
{
	public class TableCardFactory : MonoBehaviour
	{
		[SerializeField] private RectTransform tableContent;
		[SerializeField, AssetsOnly] private SpellCardEntity spellCardPrefab;
		[SerializeField, AssetsOnly] private CreepCardEntity creepCardMonoPrefab;

		private new Camera camera;

		public RectTransform TableContent => tableContent;

		private void Awake()
		{
			camera = Camera.main;

			GameBus.OnSpawnCard.Subscribe(this, SpawnCard);
		}

		private void Start()
		{
			tableContent.DestroyChildrenExcept<Image>();
		}

		private void SpawnCard(CardData data)
		{
			// 1. LocalSpawn
			var tableCard = CreateTableCard(data);
			if (tableCard == null)
				return;

			// 2. LocalSpawn->LocalSpendLava
			// When updating the context - don't waste lava
			if (!data.IsSpawnedByEffect && !BatchController.IsMuted)
				GameBus.LocalContext
					.GetVulcaniteByOwner(data.Owner)
					.SpendLava(data.Lava);

			// 3. LocalSpawn->PlayCardSync->LavaSync: if no manual effects
			if (data.IsSpawnedByResolver || !data.EffectsContainer.Effects.Any(x => x.IsManualPick))
				GameBus.OnSpawnConfirmed += data;

			// 4. LocalSpawn->PlayCardSync->LavaSync->PlayEffectSync: if has effects
			if (!data.IsSpawnedByResolver)
				EffectHandler.Handle(tableCard, EffectPhase.OnAfterSpawnOnTable);

			TableCardEntity CreateTableCard(CardData card)
			{
				return card.Type == CardType.Creep
					? SpawnCreep(card)
					: SpawnSpell(card);
			}
		}

		private int GetAliveTableCardsCount(Owner owner)
		{
			return GameBus.LocalContext.GetAliveTableCardsByOwner(owner).Count();
		}

		public bool IsMaxOwnedCardsOnTable(CardData data)
		{
			return data.Type == CardType.Creep && GetAliveTableCardsCount(data.Owner) >= SharedConfigAdapter.Config.MaxCardsOnTable;
		}

		private TableCardEntity SpawnCreep(CardData data)
		{
			if (IsMaxOwnedCardsOnTable(data) &&
			    !data.IsSpawnedByEffect) //the ability to exceed the limit for effects: reborn, destruction and spawn, etc.
				return null;

			var spawnPosition = camera.ScreenToWorldPoint(
				data.Owner == Owner.Self
					? Input.mousePosition
					: SimulateOpponentPlayPosition());

			spawnPosition.y = -10;

			var creep = Instantiate(creepCardMonoPrefab, spawnPosition, Quaternion.identity, tableContent);
			creep.Init(data);

			RRLogger.Log($"[{"Logic".Green().Bold()}] Spawn Creep {data.Title}, UID = {data.UID}, effects: "
			             + string.Join(", ", data.EffectsContainer).Lightblue());

			return creep;
		}

		private Vector3 SimulateOpponentPlayPosition()
		{
			return Screen.height * Vector3.up + Random.Range(-.8f, .8f) * Screen.width * Vector3.right;
		}

		private TableCardEntity SpawnSpell(CardData data)
		{
			var spell = Instantiate(spellCardPrefab);
			spell.Init(data);

			RRLogger.Log($"[{"Logic".Green().Bold()}] Spawn Spell {data.Title}, UID = {data.UID}, effects: "
			             + string.Join(", ", data.EffectsContainer).Lightblue());

			return spell;
		}
	}
}