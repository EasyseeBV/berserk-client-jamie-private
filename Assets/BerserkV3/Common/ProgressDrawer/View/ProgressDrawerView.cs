using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.ProgressDrawer
{
	public class ProgressDrawerView : MonoBehaviour, IProgressDrawerView
	{
		[SerializeField] protected RawImage Background;
		[SerializeField] protected ProgressDawerBarLayout ProgressDrawerBarLayout;
		[SerializeField] protected ProgressDrawerDefaultLayout DefaultProgressLayout;
		[SerializeField] protected ProgressDrawerVersusLayout VersusProgressLayout;
		public IProgressDawerBarLayout ProgressBarLayout => ProgressDrawerBarLayout;
		private IProgressDrawerLayout current;

		private void Awake()
		{
			DefaultProgressLayout.CloseAsync(true).Forget();
			VersusProgressLayout.CloseAsync(true).Forget();
			SetProgressType(ProgressType.Default);
			CloseAsync(true).Forget();
		}

		public void SetProgressType(ProgressType value)
		{
			current = value switch
			{
				ProgressType.Default => DefaultProgressLayout,
				ProgressType.Versus => VersusProgressLayout,
				_ => throw new NotImplementedException($"Unknown {nameof(ProgressType)}:{value}")
			};
		}

		public async UniTask ShowAsync(CancellationToken token = default, params object[] args)
		{
			await (current?.ShowAsync(token, args) ?? UniTask.CompletedTask);
			await ProgressDrawerBarLayout.ShowAsync(token);
			gameObject.SetActive(true);
		}

		public async UniTask CloseAsync(bool force = false, CancellationToken token = default, params object[] args)
		{
			await ProgressDrawerBarLayout.CloseAsync(force, token);
			await (current?.CloseAsync(force, token, args) ?? UniTask.CompletedTask);
			gameObject.SetActive(false);
		}
	}
}