using BerserkV3.Common.DataBase;
using BerserkV3.Common.Network;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Authorization;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;

namespace BerserkV3.Lobby.UI.Leagues
{
	public partial class LobbyLeagueInfoView : BaseView
	{
		private int currentHash = 0;
		private string key;

		protected override void OnAwake()
		{
			var baseKey = SerializeKeyHelper.LAST_LEAGUE_INFO;
			key = $"{baseKey}_{EnvironmentSwitcher.CurrentEnvironment}_{User.Id}";
			
			var title = GameDataBaseAdapter.Instance.GetLocalization("LeaguePopUpTitle");
			var description = GameDataBaseAdapter.Instance.GetLocalization("LeaguePopUpDescription");

			currentHash = (title + description).GetStableHashCode();

			TitleInfoText.text = title;
			ContentText.text = description;
			
			OkBtn.Subscribe(CloseAndSave);
		}

		public void TryToShow()
		{
			if(TutorialAdapter.Application is IBerserkTutorialApplication app)
				if(!app.IsCompleted())
					return;
			
			if (!SerializeHelperAdapter.Service.HasKey(key) ||
			    SerializeHelperAdapter.Service.Get(key) != currentHash.ToString())
				Show();
		}

		private void CloseAndSave()
		{
			SerializeHelperAdapter.Service.Patch(key, currentHash.ToString());
			SerializeHelperAdapter.Service.Save();
			Close();
		}
	}
}