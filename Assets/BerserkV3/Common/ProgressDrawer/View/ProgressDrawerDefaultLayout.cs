using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.ProgressDrawer
{
	public class ProgressDrawerDefaultLayout : MonoBehaviour, IProgressDrawerLayout
	{
		[SerializeField] protected Image FadeImage;
		[SerializeField] protected TextMeshProUGUI LoadingText;
		public virtual ProgressType Type => ProgressType.Default;
		
		public virtual UniTask ShowAsync(CancellationToken token = default, params object[] args)
		{
			gameObject.SetActive(true);
			return UniTask.CompletedTask;
		}

		public virtual UniTask CloseAsync(bool force = false, CancellationToken token = default, params object[] args)
		{
			gameObject.SetActive(false);
			return UniTask.CompletedTask;
		}
	}
}