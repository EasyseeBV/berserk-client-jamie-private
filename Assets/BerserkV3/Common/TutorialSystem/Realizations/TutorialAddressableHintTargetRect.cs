using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.Core.Serialization;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BerserkV3.Common.TutorialSystem
{

	public class TutorialAddressableHintTargetRect : TutorialHintTargetRect
	{
		[Tooltip("Would be filled by Addressables while the hint is running/closed")]
		[SerializeField] private UnitySerializedDictionary<Object, string> addressablesAssets = new();

		private CancellationTokenSource updateProcess;

		public override async Task InvokeAsync(ITutorialHintEntity hint)
		{
			updateProcess?.Cancel();
			updateProcess?.Dispose();
			updateProcess = new CancellationTokenSource();

			await UniTask.WhenAll(addressablesAssets.Select(async kpv =>
			{
				var (target, resourceId) = kpv;
				if (updateProcess == null)
					return;

				try
				{
					await target.LoadResourceAsync(resourceId, updateProcess.Token);
					if (updateProcess?.Token is {IsCancellationRequested:false} 
					    && target is Component component && component && component.gameObject)
						component.gameObject.SetActive(true);
				}
				catch
				{
					Release();
				}
			}).ToArray());
			
			await base.InvokeAsync(hint);
		}

		public override Task CloseAsync(ITutorialHintEntity hint)
		{
			Release();
			return base.CloseAsync(hint);
		}

		public override Task RemovedAsync()
		{
			Release();
			return base.RemovedAsync();
		}

		protected virtual void Release()
		{
			updateProcess?.Cancel();
			updateProcess?.Dispose();
			updateProcess = null;

			addressablesAssets?.Keys.ForEach(obj =>
			{
				obj.ReleaseResource();
				if (obj is Component component && component && component.gameObject)
					component.gameObject.SetActive(false);
			});
		}

		protected override void OnDestroy()
		{
			Release();
			base.OnDestroy();
		}
	}

}