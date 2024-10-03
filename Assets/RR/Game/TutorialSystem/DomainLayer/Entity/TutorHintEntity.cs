using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystem.Domain.Data;
using RR.Game.TutorialSystem.Event;
using UnityEngine;

namespace RR.Game.TutorialSystem.Domain
{
	public class TutorHintEntity
	{
		private readonly TutorHintData data;

		private readonly List<UnmaskHint> unmaskHints = new();
		public bool IsCompleted { get; private set; }
		public DelayInvoke DelayInvoke { get; }
		public IReadOnlyCollection<UnmaskHint> UnmaskHints => unmaskHints;
		public Vector3 PopupPosition => data.PopupPosition;
		public string Id => data.Id;

		public string NextTutorHintId => data.NextTutorHintId;
		public string[] DependentHintsIdForCompletion => data.DependentHintsIdForCompletion;
		public string[] RequiredCompletedHintIds => data.RequiredCompletedHintIds;
		public string[] Meta => data.Meta;
		public string PlaceUIName => data.PlaceUIName;
		public string Title => data.Title;
		public string Text => data.Text;
		public bool RequiredPause => data.RequiredPause;
		public bool RequiredUnmask => unmaskHints.Any(x => x.IsUnmask);
		public bool IsAttach => data.IsAttach;
		public bool IsPopupArrowShown => data.IsPopupArrowShown;
		public bool IsNextButtonShown => data.IsNextButtonShown;

		public bool IsPointerArrow => data.IsPointerArrow;
		public object PointerArrowFrom => data.PointerArrowFrom;
		public Vector3 PointerArrowFromOffset => data.PointerArrowFromOffset;
		public object PointerArrowTo => data.PointerArrowTo;
		public Vector3 PointerArrowToOffset => data.PointerArrowToOffset;
		public bool IsNeedSyncToServer => data.IsNeedSyncToServer;
		public bool IsRequiredBackHider => data.IsRequiredBackHider;

		public TutorHintEntity(TutorHintData hintData, bool isCompleted)
		{
			data = hintData;

			data.Meta.ForEach((meta, i) =>
			{
				try
				{
					var isUnmask = data.IsUnmasks[i];
					var hintVector = isUnmask
						? new HintVector(data.Offsets[i], data.FixedRects[i])
						: HintVector.Zero;

					unmaskHints.Add(new UnmaskHint(meta, hintVector, isUnmask));
				}
				catch (Exception exception)
				{
					RRLogger.Error(exception, $"From hint id {Id}");
					unmaskHints.Add(new UnmaskHint(meta, HintVector.Zero, false));
				}
			});

			DelayInvoke = new DelayInvoke(data.DelayBeforeInvoke, data.DelayAfterInvoke);
			IsCompleted = isCompleted;
		}

		public bool IsAllowedToInvoke()
		{
			return !IsCompleted;
		}

		public void DelayedInvoke()
		{
			DOVirtual.DelayedCall(DelayInvoke.DelayBeforeInvoke, Invoke);
		}

		public void Close()
		{
			TutorBus.OnHintClosed += this;
			RRLogger.Log($"{nameof(TutorHintEntity)} with id {Id} closed.");
		}

		public void MarkCompleted()
		{
			IsCompleted = true;
		}

		public void Reset()
		{
			IsCompleted = false;
		}

		private void Invoke()
		{
			TutorBus.OnHintInvoked += this;
			RRLogger.Log($"{nameof(TutorHintEntity)} with id {Id} invoked.");
		}
	}
}