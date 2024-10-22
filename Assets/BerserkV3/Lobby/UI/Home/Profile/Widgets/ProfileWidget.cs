using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class ProfileWidget : MonoBehaviour
	{
		[SerializeField] private VulcaniteWidget vulcaniteWidget;
		[SerializeField] private TMP_Text mmrText;
		[SerializeField] private TMP_Text nicknameText;

		public async UniTask InitAsync(string nickname, string heroArtUrl, string borderId, CancellationToken cancellationToken = default)
		{
			nicknameText.text = nickname;
			await vulcaniteWidget.InitAsync(heroArtUrl, borderId, cancellationToken);
		}
	}
}