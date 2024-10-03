using System.Threading;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
	public partial class CardStatisticView : BaseView, IPreviewable
	{
		[SerializeField] private CardHandArtView HandCardArtView;
		private CancellationTokenSource loadArt;
		protected override void Start()
		{
			base.Start();
			PreviewSystemAdapter.Instance.Registration(this);
		}

		public void Init(CardData data, string counterValue)
		{
			PreviewData = data.ToPreviewData();
			loadArt?.Cancel();
			loadArt?.Dispose();
			loadArt = new CancellationTokenSource();
			SetActive(CounterArt, false);
			HandCardArtView.SetActive(false);
			HandCardArtView.SetupAsync(data.ToCardDataAdapter(), loadArt.Token)
				.ContinueWith(() =>
				{
					HandCardArtView.SetActiveShine(false);
					HandCardArtView.SetActive(true);
					SetActive(CounterArt, !string.IsNullOrEmpty(counterValue));
					Set(CounterTxt, $"x{counterValue}");
				}).Forget();
		}

		private void OnDestroy()
		{
			loadArt?.Cancel();
			loadArt?.Dispose();
			loadArt = null;
			PreviewData = null;
			HandCardArtView.Release();
			PreviewSystemAdapter.Instance.UnRegistration(this);
		}
		
		#region Previewable

		public GameObject TargetView => gameObject;

		public IPreviewData PreviewData { get; private set; }

		public IPreviewSetting PreviewSettings { get; } = new PreviewSettings(PreviewType.Fit);
		public bool CanPreview() => TargetView && PreviewData != null;

		#endregion
	}
}