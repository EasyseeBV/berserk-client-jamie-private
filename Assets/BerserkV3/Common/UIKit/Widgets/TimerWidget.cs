using System;
using System.Collections;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public partial class TimerWidget : BaseView
	{
		[SerializeField] protected TextMeshProUGUI TimerText;
		[SerializeField] protected Image FilledImage;
		private Coroutine routine;

		protected override void OnDisable()
		{
			Clear();
			base.OnDisable();
		}

		public void SetTimerText(string value)
		{
			Set(TimerText, value);
		}

		public void SetTimer(float value, Action onComplete = null)
		{
			SetTimerText($"{Math.Ceiling(value)}");
			SetActive(value > 0);
			if (routine != null) 
				StopCoroutine(routine);

			if (value <= 0)
			{
				SetTimerText("");
				onComplete?.Invoke();
				return;
			}
		
			routine = StartCoroutine(StarTimer(value, onComplete));
		}

		public void SetTimerProgress(float value01)
		{
			FilledImage.fillAmount = value01;
		}
	
		public void SetActive(bool value)
		{
			SetActive(this, value);
		}

		public void Clear()
		{
			SetTimerText("");
			StopAllCoroutines();
			routine = null;
		}

		private IEnumerator StarTimer(float value, Action onComplete = null)
		{
			var maxProgress = value;
			while (Application.isPlaying)
			{
				value = Math.Max(0, value-Time.deltaTime);
				SetTimerText($"{Math.Ceiling(value)}");
				SetTimerProgress(value / maxProgress);

				if (value <= 0)
				{
					SetTimerProgress(0);
					onComplete?.Invoke();
					break;
				}

				yield return null;
			}
		}
	}
}
