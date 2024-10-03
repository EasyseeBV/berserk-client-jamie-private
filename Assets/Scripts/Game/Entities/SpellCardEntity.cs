using Audio;
using Events;
using DG.Tweening;
using RR.Core.DebugSystem;
using Vulcan.Audio;
using Vulcan.Data;
using Game.Effect_System;

namespace Game.Entities
{
	public class SpellCardEntity : TableCardEntity
	{
		protected override void OnInit()
		{
			GameBus.OnPassingTurned.Unsubscribe(OnPassingTurned);

			base.OnInit();

			transform.DOSpiral(1f, frequency: 100, depth: 10);
			AudioController.Play(Clip.Spell_Release);
		}

		protected override void RemoveFromContext()
		{
			Destroy(gameObject, 1f);
		}

		protected override void AddToContext()
		{
			GameBus.LocalContext.AddGraveyardCard(Data);

			GameBus.UpdateGraveyard += true;
		}

		protected override void ConfirmSpawnCard()
		{
			AddToContext();
			base.ConfirmSpawnCard();
			EffectHandler.Handle(this, EffectPhase.BeforeDead);
			EffectHandler.Handle(this, EffectPhase.AfterDead);
			Destroy();
		}

		public override void SetTurn(bool myTurn)
		{
			RRLogger.Error($"Spell [{Data.Owner} : {Data.Title}] shouldn't be triggered by OnNextRound!");
		}

		protected override void OnPassingTurned()
		{
			RRLogger.Error($"Spell [{Data.Owner} : {Data.Title}] shouldn't be triggered by OnPassingTurned!");
		}

		protected override void AddEffectsPostSpawn() { }
	}
}