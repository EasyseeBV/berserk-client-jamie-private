using System.Threading;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class CardWidget : MonoBehaviour
	{
		[SerializeField] private CardHandArtView cardHandArtView;
		
		public async UniTask InitAsync(CardData data, CancellationToken cancellationToken = default)
		{
			cardHandArtView.SetActive(false);
			await cardHandArtView.SetupAsync(data.ToCardDataAdapter(), cancellationToken);
				
			cardHandArtView.SetActiveShine(false);
			cardHandArtView.SetActive(true);
		}
		
		public void Show()
		{
			cardHandArtView.SetActive(true);
		}
		
		public void Hide()
		{
			cardHandArtView.SetActive(false);
		}
		
		private void OnDestroy()
		{
			cardHandArtView.Release();
		}
	}
}