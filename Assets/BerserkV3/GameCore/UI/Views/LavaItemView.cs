using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public class LavaItemView : MonoBehaviour
	{
		[SerializeField] private RawImage background;
		[SerializeField] private RawImage foreGround;

		public void SetActive(bool value)
		{
			gameObject.SetActive(value);
		}

		public void FadeInAnimate()
		{
			foreGround.DOKill();
			foreGround.DOColor(Color.black, 0.75f);
			foreGround.DOFade(0.2f, .75f);
		}

		public void FadeOutAnimate()
		{
			foreGround.DOKill();
			foreGround.DOColor(Color.white, 0.75f);
			foreGround.DOFade(1, .75f);
		}
		
		private void OnEnable()
		{
			background.LoadResourceAsync($"Vulcanite_Attribute_Lava_Empty").Forget();
			foreGround.LoadResourceAsync($"Vulcanite_Attribute_Lava").Forget();
		}

		private void OnDisable()
		{
			background.ReleaseResource();
			foreGround.ReleaseResource();
		}

		private void OnDestroy()
		{
			background.ReleaseResource();
			foreGround.ReleaseResource();
		}
	}
}