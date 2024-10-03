using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Events;
using Game.Entities;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Vulcan.Audio;
using Vulcan.Data;

namespace Game.Effect_System.Target
{
	public class TargetResolver : MonoBehaviour // Singleton but without DontDestroyOnLoad
	{
		[SerializeField, AssetsOnly] private ManualTargetPicker arrowLinePrefab;
		[SerializeField] private Transform arrowLineContainer;
		[SerializeField] private Color defaultTargetColor = Color.white;
		[SerializeField] private Color requestingColor; // blue #3b6fff
		[SerializeField] private Color friendTargetColor; //green #00C503
		[SerializeField] private Color enemyTargetColor; //red #DD1B00

		public static ConditionHandler ConditionHandler => instance.conditionHandler;

		private readonly ConditionHandler conditionHandler = new();

		private ManualTargetPicker targetPicker;

		private Action onPicked;
		private Action onCanceled;

		private readonly List<PickInfo> statePicksInfo = new();
		private PickInfo pickInfo;

		private static TargetResolver instance;

		private void Awake()
		{
			if (instance != null)
				Destroy(gameObject);

			instance = this;
			targetPicker = Instantiate(arrowLinePrefab, arrowLineContainer);
			targetPicker.gameObject.SetActive(false);
			targetPicker.OnSelectionEnd += Pick;
			targetPicker.OnSelectionCancel += CancelPick;
		}

		private void Start()
		{
			GameBus.OnEntityTarget.Subscribe(this, _ => pickInfo != null, TryFixTarget);
			GameBus.CurrentRound.Subscribe(this, _ => pickInfo != null, CancelPick);
			GameBus.OnPassingTurned.Subscribe(this, () => pickInfo != null, CancelPick);
		}

		public static void AutoAssignTargets(PickInfo pickInfo)
		{
			if (pickInfo == null)
			{
				RRLogger.Error($"[{"TargetResolver.AutoAssignTargets".Red().Bold()}] PickInfo is missing");
				return;
			}

			if (pickInfo.EffectState == null)
			{
				RRLogger.Error($"[{"TargetResolver.AutoAssignTargets".Red().Bold()}] EffectState is missing");
				return;
			}

			if (pickInfo.EffectState.EffectData.Aoe == EffectAoe.Single)
				switch (pickInfo.EffectState.EffectData.TargetMod)
				{
					case EffectTargetMod.Random:
						pickInfo.Targets =
							GetValidateTargets(ConditionHandler.GetRandomAllowedTarget(pickInfo.From,
								pickInfo.EffectState.EffectData));
						return;

					case EffectTargetMod.Self:
						pickInfo.Targets = GetValidateTargets(pickInfo.From);
						return;

					case EffectTargetMod.RandomExceptSelf:
						pickInfo.Targets =
							GetValidateTargets(ConditionHandler.GetRandomAllowedTargetExceptSelf(pickInfo.From,
								pickInfo.EffectState.EffectData));
						return;

					case EffectTargetMod.None:
						pickInfo.Targets = Array.Empty<IInteractiveEntity>();
						return;

					case EffectTargetMod.EntityAttackPicked:
						pickInfo.Targets = GetValidateTargets(pickInfo.Targets);
						return;

					default:
						RRLogger.Error(
							$"[{"TargetResolver".Red().Bold()}] — {pickInfo.EffectState.EffectData.TargetMod} unsupported to auto assign targets");
						return;
				}

			switch (pickInfo.EffectState.EffectData.Aoe)
			{
				case EffectAoe.AoeFromTarget: // todo: implement
					RRLogger.Log("Not implemented. Performing simple AOE".Red());
					pickInfo.Targets = GetValidateTargets(ConditionHandler
						.GetAOETargets(pickInfo.From, pickInfo.EffectState.EffectData).ToArray());
					return;

				case EffectAoe.AoeAll:
					pickInfo.Targets = GetValidateTargets(ConditionHandler
						.GetAOETargets(pickInfo.From, pickInfo.EffectState.EffectData).ToArray());
					return;

				case EffectAoe.AoeAllExceptSelf:
					pickInfo.Targets = GetValidateTargets(ConditionHandler
							.GetAOETargets(pickInfo.From, pickInfo.EffectState.EffectData).ToArray())
						.Where(x => !x.DataBase.Equals(pickInfo.From.DataBase))
						.ToArray();
					return;

				case EffectAoe.AoeAllExceptTarget:
					RRLogger.Log("Not implemented. Performing simple AOE".Red());
					pickInfo.Targets = GetValidateTargets(ConditionHandler
						.GetAOETargets(pickInfo.From, pickInfo.EffectState.EffectData).ToArray());
					return;

				default:
					RRLogger.Error(
						$"[{"TargetResolver".Red().Bold()}] — {pickInfo.EffectState.EffectData.Aoe} unsupported to auto assign targets");
					return;
			}
		}

		public static void GetManualTarget(List<PickInfo> picksInfo, Action onPicked, Action onCanceled = null)
		{
			instance.targetPicker.Cancel();
			instance.DisposeManualTarget();

			var pickInfo = PreparePickInfo(picksInfo, onPicked, onCanceled);
			if (pickInfo == null)
			{
				RRLogger.Error($"[{"TargetResolver.GetManualTarget".Red().Bold()}] PickInfo is missing");
				onCanceled?.Invoke();
				return;
			}

			instance.targetPicker.SetUp(pickInfo.From);

			GameBus.TargetSelecting += true;
		}

		private static IInteractiveEntity[] GetValidateTargets(params IInteractiveEntity[] targets)
		{
			return targets?.Where(x => x != null).ToArray() ?? Array.Empty<IInteractiveEntity>();
		}

		private static PickInfo PreparePickInfo(List<PickInfo> picksInfo, Action onPicked, Action onCanceled)
		{
			picksInfo = picksInfo ?? new List<PickInfo>();
			var pickInfo = picksInfo.FirstOrDefault(x => x.EffectState == null
			                                             || x.EffectState.EffectData.IsManualPick); // any manual effect

			instance.statePicksInfo.Clear();
			instance.statePicksInfo.AddRange(picksInfo);
			instance.pickInfo = pickInfo;
			instance.onPicked = onPicked;
			instance.onCanceled = onCanceled;

			if (pickInfo?.From?.View == null)
			{
				RRLogger.Error(
					$"[{"TargetResolver.PreparePickInfo".Red().Bold()}] PickInfo is missing, does not find any manual target");
				return pickInfo;
			}

			pickInfo.From.View.Unselect();
			pickInfo.From.View.SelectOnRequestingTarget(instance.requestingColor);
			return pickInfo;
		}

		private void Pick()
		{
			if (pickInfo == null)
			{
				RRLogger.Error($"[{"TargetResolver.Pick".Red().Bold()}] PickInfo is null");
				return;
			}

			if (pickInfo.Targets == null || !pickInfo.Targets.Any())
			{
				CancelPick();
				return;
			}

			statePicksInfo.Where(x => x.EffectState != null && x.EffectState.EffectData.IsManualPick)
				.ForEach(x => x.Targets = GetValidateTargets(pickInfo.Targets));

			onPicked?.Invoke();
			DisposeManualTarget();
		}

		private void DisposeManualTarget()
		{
			if (pickInfo != null)
			{
				if (pickInfo.From?.View != null)
					pickInfo.From.View.SelectOnRequestingTarget(defaultTargetColor);

				pickInfo.Targets?.ForEach(UnselectTargetView);
			}

			pickInfo = null;
			onPicked = null;
			onCanceled = null;
			statePicksInfo?.Clear();

			GameBus.TargetSelecting += false;
		}

		private void TryFixTarget(IInteractiveEntity target)
		{
			if (GameBus.OnEntityTarget.PrevValue != null)
				CancelFixTarget(GameBus.OnEntityTarget.PrevValue);

			if (target == null)
				return;

			if (pickInfo == null)
			{
				RRLogger.Error($"[{"TargetResolver.TryFixTarget".Red().Bold()}] PickInfo is null");
				return;
			}

			RRLogger.Log($"{pickInfo.From.RectTransform.name} enter over {target.RectTransform.name}".Pink());

			//Temp
			if (pickInfo.EffectState != null)
			{
				if (!conditionHandler.AllowedTarget(pickInfo.From, target, pickInfo.EffectState.EffectData))
					return;
			}
			else
			{
				if (!conditionHandler.AllowedTarget(pickInfo.From, target))
					return;
			}

			var selectedColor = pickInfo.From.DataBase.Owner == target.DataBase.Owner
				? friendTargetColor
				: enemyTargetColor;

			AudioController.Play(Clip.Arrow_Select);
			pickInfo.Targets = GetValidateTargets(target);

			target.View.Select(selectedColor);
		}

		private void CancelFixTarget(IInteractiveEntity target)
		{
			if (!RequiredClearTarget())
				return;

			if (pickInfo == null)
			{
				RRLogger.Error($"[{"TargetResolver.CancelFixTarget".Red().Bold()}] PickInfo is null");
				return;
			}

			AudioController.Play(Clip.Arrow_Select);
			pickInfo.Targets = Array.Empty<IInteractiveEntity>();
			RRLogger.Log($"{pickInfo.From.RectTransform.name} exit over {target.RectTransform.name}".Pink());

			UnselectTargetView(target);

			bool RequiredClearTarget()
			{
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				return Input.GetTouch(0).phase < TouchPhase.Ended;
#else
				return true;
#endif
			}
		}

		private void CancelPick()
		{
			onCanceled?.Invoke();
			DisposeManualTarget();
		}

		private void UnselectTargetView(IInteractiveEntity target)
		{
			if (target == null)
			{
				RRLogger.Error($"[{"TargetResolver.UnselectTargetView".Red().Bold()}] Target enitity is null");
				return;
			}

			if (pickInfo == null)
			{
				RRLogger.Error($"[{"TargetResolver.UnselectTargetView".Red().Bold()}] PickInfo is null");
				return;
			}

			if (pickInfo.From == null)
			{
				RRLogger.Error($"[{"TargetResolver.UnselectTargetView".Red().Bold()}] PickInfo.From is null");
				return;
			}

			if (target.IsCanAttack && target != pickInfo.From)
			{
				target.View.Select(Color.white);
				return;
			}

			target.View.Unselect();
		}
	}
}