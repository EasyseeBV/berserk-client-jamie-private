using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystem.Domain;
using RR.Game.TutorialSystem.Domain.Data;
using RR.Game.TutorialSystem.Event;
using RR.UI.FrameSystem;
using UnityEngine;

namespace RR.Game.TutorialSystem.Presentation.UI
{
	public partial class TutorialUIPanel : BaseView
	{
		private List<HintTarget> hintTargets = new();

		protected override void OnAwake()
		{
			//when integrating new tutor hint view, add type checking here
			TutorBus.OnHintInvoked.Subscribe(this, ShowHint);
			TutorBus.OnHintClosed.Subscribe(this, CloseHint);
		}

		public void AddTarget(HintTarget hintTarget)
		{
			hintTargets.Add(hintTarget);
		}

		private void ShowHint(TutorHintEntity hint)
		{
			OnShowHint(hint);

			var hintTargetsByHint = FindHintTargets(hint);
			if (!hintTargetsByHint.Any())
			{
				hint.DelayedInvoke();
				RRLogger.Error($"{nameof(hintTargetsByHint)} not found for hint: {hint.Id}");
				return;
			}

			TutorHintView.SetUpUnmasks(hint, hintTargetsByHint);
			TutorHintView.SetUpHintPopup(hint);
			UpdatePointerArrow(hint);

			DOVirtual.DelayedCall(hint.DelayInvoke.DelayAfterInvoke,
					() => TutorHintView.BackgroundButton.interactable = true)
				.SetUpdate(true);

			hintTargetsByHint.ForEach(hintTarget => hintTarget.SetUp(TutorHintView));
			TutorHintView.Show(noAnimation: true);
			if (hintTargetsByHint.Any(x => x.IsClickToCloseHint))
				return;

			var target = hintTargetsByHint.First();
			target.SubscribeOnContinue(TutorHintView.BackgroundButton);
			target.SubscribeOnContinue(TutorHintView.NextButton);
		}

		private void UpdatePointerArrow(TutorHintEntity hint)
		{
			if (!hint.IsPointerArrow)
				return;

			var arrowPositionFrom = Vector3.zero;
			var arrowPositionTo = Vector3.zero;
			if (TryGetPointerArrowFromPosition(hint, ref arrowPositionFrom) && TryGetPointerArrowToPosition(hint, ref arrowPositionTo))
			{
				var showPointerArrow =
						new TutorDynamicArrowData(arrowPositionFrom + hint.PointerArrowFromOffset,
						arrowPositionTo + hint.PointerArrowToOffset);
				TutorBus.OnTutorArrowShowed += showPointerArrow;
			}
		}

		protected virtual HintTarget[] FindHintTargets(TutorHintEntity hint)
		{
			return hintTargets
				.Where(x => x != null && x.HintId == hint.Id)
				.ToArray();
		}

		protected virtual bool TryGetPointerArrowFromPosition(TutorHintEntity hint, ref Vector3 arrowPositionFrom)
		{
			if (hint.PointerArrowFrom.GetType() == typeof(string))
				return false;

			if (TryGetVector3FromJson(hint.PointerArrowFrom, ref arrowPositionFrom))
				return true;

			return false;
		}

		protected virtual bool TryGetPointerArrowToPosition(TutorHintEntity hint, ref Vector3 arrowPositionTo)
		{
			if (hint.PointerArrowTo.GetType() == typeof(string))
				return false;

			if (TryGetVector3FromJson(hint.PointerArrowTo, ref arrowPositionTo))
				return true;

			return false;
		}

		protected bool TryGetVector3FromJson(object objectPositionJson, ref Vector3 positon)
		{
			try
			{
				positon = JsonConvert.DeserializeObject<Vector3>(objectPositionJson.ToString());
				return true;
			}
			catch (Exception ex)
			{
				RRLogger.Error($"Can not convert to Vector3. Incorrect format {nameof(objectPositionJson)}: {objectPositionJson}. Exeption: {ex}");
				return false;
			}
		}

		protected virtual void OnShowHint(TutorHintEntity hint)
		{
		}

		protected virtual void OnCloseHint(TutorHintEntity hint)
		{
		}

		private void CloseHint(TutorHintEntity hint)
		{
			OnCloseHint(hint);

			TutorHintView.Close(noAnimation: true);
		}
	}
}