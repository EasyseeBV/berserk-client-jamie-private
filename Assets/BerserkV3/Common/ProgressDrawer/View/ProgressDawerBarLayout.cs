using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.ProgressDrawer
{
	public class ProgressDawerBarLayout : MonoBehaviour, IProgressDawerBarLayout
	{
		[SerializeField] protected TextMeshProUGUI LoadingProgressText;
		[SerializeField] protected TextMeshProUGUI TooltipText;
		[SerializeField] protected Image ProgressFill;
		[SerializeField] protected float smoothTime = 3f;
		[SerializeField] protected string format = "{0}<size=25>%";
		private string overrideFormat;
		private Tween progressTween;
		
		public void SetActiveTooltip(bool value)
		{
			if (TooltipText)
				TooltipText.gameObject.SetActive(value);
		}

		public void SetTooltipText(string value)
		{
			if (TooltipText)
				TooltipText.SetText(value ?? string.Empty);
		}

		public void SetProgressFormat(string value)
		{
			overrideFormat = value;
		}

		public void SetProgress(float value)
		{
			if (!ProgressFill)
				return;

			if (value == 0)
			{
				ProgressFill.DOKill();
				ProgressFill.fillAmount = value;
				UpdateProgressText();
				return;
			}

			progressTween?.Kill();
			var lastValue = ProgressFill.fillAmount;
			var deltaValue = Mathf.Abs(Mathf.Clamp01(value - lastValue));

			progressTween = ProgressFill
				.DOFillAmount(Mathf.Clamp01(value), deltaValue * smoothTime)
				.OnUpdate(UpdateProgressText)
				.Play();
		}

		public UniTask ShowAsync(CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
				return UniTask.CompletedTask;

			gameObject.SetActive(true);
			return UniTask.CompletedTask;
		}

		public async UniTask CloseAsync(bool force = false, CancellationToken token = default)
		{
			if (!force && progressTween != null)
				await progressTween.ToUniTask(cancellationToken: token).SuppressCancellationThrow();

			if (token.IsCancellationRequested)
				return;

			progressTween?.Kill();
			progressTween = null;
			overrideFormat = null;
			gameObject.SetActive(false);
			await UniTask.Yield();
		}

		private void UpdateProgressText()
		{
			if (LoadingProgressText && ProgressFill)
				LoadingProgressText.SetText(string.Format(overrideFormat ?? format,
					Mathf.CeilToInt(ProgressFill.fillAmount * 100f)));
		}
	}
}