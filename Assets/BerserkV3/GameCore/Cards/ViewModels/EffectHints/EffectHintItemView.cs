using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.Cards.EffectHints
{
	public class EffectHintItemView : MonoBehaviour
	{
		[SerializeField] private RawImage effectImage;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private TextMeshProUGUI descriptionText;
		private bool releasePrevious;
	
		public async UniTask SetupAsync(string imageUrl,string title, string description, CancellationToken token)
		{
			titleText.text = title;
			descriptionText.text = description;
			await effectImage.LoadResourceAsync(imageUrl, token, releasePrevious);
			releasePrevious = true;
		}
	
		private void OnDestroy()
		{
			if (releasePrevious)
				effectImage.ReleaseResource();
		}
	}
}
