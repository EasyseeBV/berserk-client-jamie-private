using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.Cards.EffectHints
{
	public class EffectHintImageItemView : MonoBehaviour
	{
		[SerializeField] private RawImage effectImage;
		private bool releasePrevious;
		private GameObject selfObject;
		public bool IsActive => selfObject && selfObject.activeSelf;
		
		public async UniTask ShowAsync(string imageUrl, CancellationToken token)
		{
			selfObject ??= gameObject;
			
			await effectImage.LoadResourceAsync(imageUrl, token, releasePrevious);
			releasePrevious = true;
			SetActive(true);
		}

		public void SetActive(bool value)
		{			
			if (selfObject)
				selfObject.SetActive(value);
		}

		private void OnDestroy()
		{
			if (releasePrevious)
				effectImage.ReleaseResource();
		}
	}
}