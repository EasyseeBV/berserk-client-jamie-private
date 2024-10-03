using System.Threading;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.TutorialSystem;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public interface IDeckHolderView
	{
		Owner Owner { get; }
		GameObject TargetView { get; }
		void SetCountText(string value);

		UniTask SetArt(string artUrl, CancellationToken token);
	}

	public partial class DeckHolderView : BaseView, IDeckHolderView
	{
		[SerializeField] private RawImage ArtImage;
		[SerializeField] private TextMeshProUGUI CounterText;
		[SerializeField] private Owner owner = Owner.None;
		
		public Owner Owner => owner;
		public GameObject TargetView => gameObject;

		protected override void OnAwake()
		{
			base.OnAwake();
			this.SetHintTarget($"{TutorialTrigger.GameDeck}_{owner}").SetTransitionFactorSize().Init();
		}

		public void SetCountText(string value)
		{
			Set(CounterText, value);
		}

		public UniTask SetArt(string artUrl, CancellationToken token)
		{
			return ArtImage.LoadResourceAsync(artUrl, token);
		}

		private void OnDestroy()
		{
			ArtImage.ReleaseResource();
		}
	}
}