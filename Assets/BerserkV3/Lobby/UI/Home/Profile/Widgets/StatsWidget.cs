using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class StatsWidget : MonoBehaviour
	{
		[SerializeField] private TMP_Text totalText;
		[SerializeField] private TMP_Text winText;
		[SerializeField] private TMP_Text winrateText;

		public void Init(int totalGames, int totalWins, string winrateFormat)
		{
			totalText.text = totalGames.ToString();
			winText.text = totalWins.ToString();
			winrateText.text = winrateFormat;
		}
	}
}