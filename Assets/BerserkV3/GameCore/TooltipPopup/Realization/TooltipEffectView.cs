using System.Threading;
using BerserkV3.GameCore.TooltipPopup.Data;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipEffectView : MonoBehaviour
	{
		[SerializeField] private TMP_Text title;
		[SerializeField] private TMP_Text description;
		[SerializeField] private RawImage effectImage;
		private bool releasePrevious;

		public async UniTask InitAsync(TooltipEffectData data, CancellationToken ctn = default)
		{
			title.text = data.Title;
			description.text = data.Description;
			
			if (string.IsNullOrEmpty(data.IconUrl))
			{
				SetIconVisible(false);
				Clear();
				return;
			}

			await effectImage.LoadResourceAsync(data.IconUrl, ctn, releasePrevious);
			SetIconVisible(true);
			releasePrevious = true;
		}

		public void SetIconVisible(bool value)
		{
			if (effectImage)
				effectImage.gameObject.SetActive(value);
		}
		
		public void SetActive(bool value)
		{
			gameObject.SetActive(value);
		}

		public void Clear()
		{
			if (releasePrevious)
				effectImage.ReleaseResource();

			releasePrevious = false;
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}