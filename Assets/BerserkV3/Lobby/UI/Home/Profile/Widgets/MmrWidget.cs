using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class MmrWidget : MonoBehaviour
	{
		[SerializeField] private LeagueWidget leaguePrefab;

		private List<GameObject> leaguesList = new();
		
		public async UniTask AddLeagueAsync(string leagueArtId, string leagueMmr, CancellationToken cancellationToken = default)
		{
			var leagueWidget = Instantiate(leaguePrefab, gameObject.transform);
			leaguesList.Add(leagueWidget.gameObject);
			await leagueWidget.InitAsync(leagueArtId, leagueMmr, cancellationToken);
		}

		public void Clear()
		{
			foreach (var gameObject in leaguesList)
			{
				Destroy(gameObject);
			}
		}
	}
}