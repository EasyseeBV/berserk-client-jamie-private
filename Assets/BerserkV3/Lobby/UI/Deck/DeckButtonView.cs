using System;
using System.Threading;
using BerserkV3.Common.TutorialSystem;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI
{
	public partial class DeckButtonView : BaseView
	{
		[SerializeField] private Button selectButton;
		private CancellationTokenSource coatUpdate;
		private CancellationTokenSource vulcaniteUpdate;
		private CancellationTokenSource leagueFlagsUpdate;
		private CancellationTokenSource factionFlagUpdate;
		private const float HINT_SIZE_MULTIPLIER = 1.1f;
		private string HintTag => $"{transform.GetSiblingIndex()}";
		private bool selected;
		private bool warning;
		public string Id { get; set; }
		
		public void SetVulcaniteImage(string url)
		{
			var releasePreviouse = vulcaniteUpdate != null;
			vulcaniteUpdate?.Cancel();
			vulcaniteUpdate?.Dispose();
			vulcaniteUpdate = new CancellationTokenSource();
			VulcaniteImage.LoadResourceAsync(url, vulcaniteUpdate.Token, releasePreviouse).Forget();
		}

		public void SetCoatImage(string quadrantUrl)
		{
			var releasePreviouse = coatUpdate != null;
			coatUpdate?.Cancel();
			coatUpdate?.Dispose();
			coatUpdate = new CancellationTokenSource();
			CoatImage.LoadResourceAsync(quadrantUrl, coatUpdate.Token, releasePreviouse).Forget();
			CoatImage.SetHintTarget($"{TutorialTrigger.DeckVulcaniteFlag}_{HintTag}").SetTransitionFactorSize().Init().SizeScale *= HINT_SIZE_MULTIPLIER;
		}

		public void SetDeckName(string value)
		{
			if (NameText)
				Set(NameText, value);
		}

		public void SetCardCount(string value)
		{
			if(CardsCountText)
				Set(CardsCountText, value);
		}

		public void SetLeagueFlags(params string[] urls)
		{
			ClearFlags();
			leagueFlagsUpdate = new CancellationTokenSource();
			
			foreach (var artUrl in urls)
			{
				var flagInstance = Instantiate(LeagueFlag, Leagues);
				if (leagueFlagsUpdate != null)
					flagInstance.LoadResourceAsync(artUrl, leagueFlagsUpdate.Token, false).Forget();
				SetActive(flagInstance, true);
				flagInstance.SetHintTarget($"{TutorialTrigger.DeckLeagueFlags}_{HintTag}").SetTransitionFactorSize().Init().SizeScale *= HINT_SIZE_MULTIPLIER;
			}
		}

		public void SetFactionFlag(string url)
		{
			var releasePreviouse = factionFlagUpdate != null;
			factionFlagUpdate?.Cancel();
			factionFlagUpdate?.Dispose();
			factionFlagUpdate = new CancellationTokenSource();
			FactionFlag.LoadResourceAsync(url, factionFlagUpdate.Token, releasePreviouse).Forget();
			FactionFlag.SetHintTarget($"{TutorialTrigger.DeckFactionFlag}_{HintTag}").SetTransitionFactorSize().Init().SizeScale *= HINT_SIZE_MULTIPLIER;
			SetActive(FactionFlag, !string.IsNullOrEmpty(url));
		}

		public void ClearFlags()
		{
			var releasePreviouse = leagueFlagsUpdate != null;
			leagueFlagsUpdate?.Cancel();
			leagueFlagsUpdate?.Dispose();
			leagueFlagsUpdate = null;
			if (releasePreviouse)
				Leagues.GetComponentsInChildren<RawImage>().ForEach(x=> x.ReleaseResource());
			
			Leagues.DestroyChildrenExcept(LeagueFlag.transform);
		}

		public void SetSelectedAction(Action<string> value)
		{
			selectButton.onClick.AddListener(() => value?.Invoke(Id));
		}

		public void SetWarning(bool value)
		{
			warning = value;
			SetActiveOutline(value);
			SetOutlineColor(GetColorByState());
		}

		public void SetSelect(bool value)
		{
			selected = value;
			SetActiveOutline(value);
			SetOutlineColor(GetColorByState());
		}
		
		public void SetInteractable(bool value)
		{
			SetInteractable(selectButton, value);
		}

		public void Clear()
		{
			if (vulcaniteUpdate != null)
				VulcaniteImage.ReleaseResource();
			
			if (factionFlagUpdate != null)
				FactionFlag.ReleaseResource();
			
			if (coatUpdate != null)
				CoatImage.ReleaseResource();
			
			coatUpdate?.Cancel();
			coatUpdate?.Dispose();
			coatUpdate = null;
			factionFlagUpdate?.Cancel();
			factionFlagUpdate?.Dispose();
			factionFlagUpdate = null;
			vulcaniteUpdate?.Cancel();
			vulcaniteUpdate?.Dispose();
			vulcaniteUpdate = null;
			ClearFlags();
		}
		
		private void SetActiveOutline(bool value)
		{
			SetActive(OutlineImg, warning || selected || value);
		}
		
		private void SetOutlineColor(Color value)
		{
			Set(OutlineImg, value);
		}

		private Color GetColorByState()
		{
			return warning ? Color.red : Color.white;
		}

		private void OnDestroy()
		{
			Clear();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}
	}
}