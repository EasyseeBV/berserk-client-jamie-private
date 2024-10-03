using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Presentation;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Realizations
{
	public static class TutorialExtensions
	{
		public static IEnumerable<T> OrderIfPossible<T>(this IEnumerable<T> source)
		{
			return source?.OrderBy(x => x is ITutorialOrderable orderable ? orderable.Order : 100);
		}
		
		public static BaseTutorialHintTarget SetHintTarget<T>(this T component, params string[] ids) where T : Component
		{
			if (!component)
				return default;

			if (component.TryGetComponent(out BaseTutorialHintTarget target))
			{
				target.SetTriggerIds(target.Ids.Union(ids).Distinct().ToArray());
				return target;
			}
			
			target = component.TryGetComponent(out RectTransform _)
				? component.gameObject.AddComponent<TutorialHintTargetRect>()
				: component.gameObject.AddComponent<TutorialHintTarget>();
			
			target.SetTriggerIds(ids);
			return target;
		}
		
		public static void Forget<T>(this T task, Action<Exception> exception = null) where T : Task
		{
			try
			{
				task.ContinueWith(t =>
				{
					if (t.Exception != null)
					{
						RRLogger.Error(t.Exception);
						exception?.Invoke(t.Exception);
					}
				});
			}
			catch (Exception e)
			{
				exception?.Invoke(e);
				RRLogger.Error(e);
			}
		}
	}
}