using System.Threading;
using Berserk.Shared.Data.Game;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class DeckWidget : MonoBehaviour
	{
		[SerializeField] private CardWidget[] widgets;
		//[SerializeField] private Button seeMoreButton;

		public async UniTask InitAsync(CardData[] cardDatas, CancellationToken token = default)
		{
			var tasks = widgets.Select((widget, i) =>
			{
				if (i < cardDatas.Length)
				{
					widget.Show();
					return widget.InitAsync(cardDatas[i], token);
				}

				widget.Hide();
				return UniTask.CompletedTask;
			});
			
			await UniTask.WhenAll(tasks).AttachExternalCancellation(token);
		}

		public void Enable(bool value)
		{
			gameObject.SetActive(value);
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}
		
		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}