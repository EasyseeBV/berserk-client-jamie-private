using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class LeagueWidget : MonoBehaviour
	{
		[SerializeField] private ImageWidget leagueImageWidget;
		[SerializeField] private TMP_Text mmrText;

		public async UniTask InitAsync(string leagueImgId, string mmr, CancellationToken cancellationToken = default)
		{
			mmrText.text = $"MMR: {mmr}";
			await leagueImageWidget.InitAsync(leagueImgId, cancellationToken);
		}
	}
}