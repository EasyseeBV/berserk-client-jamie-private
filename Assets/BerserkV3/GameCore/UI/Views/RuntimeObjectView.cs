using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IRuntimeObjectView
	{
		RectTransform SelfContainer { get; }
		IRuntimeGameObject RuntimeGameObject { get; }
		IRuntimeData RuntimeData { get; }
		void Setup(IRuntimeGameObject runtimeGameObject);
		
		void Enable();
		
		void Disable();
	}

	public abstract class RuntimeObjectView : BaseView, IRuntimeObjectView
	{
		[SerializeField] protected RectTransform selfContainer;
		private CancellationTokenSource becameVisible;

		public RectTransform SelfContainer => selfContainer;

		public IRuntimeGameObject RuntimeGameObject { get; private set; }
		
		public IRuntimeData RuntimeData => RuntimeGameObject?.RuntimeData;

		public void Setup(IRuntimeGameObject runtimeGameObject)
		{
			RuntimeGameObject = runtimeGameObject;
			OnSetup();
		}

		public void Enable()
		{
			becameVisible?.Cancel();
			becameVisible?.Dispose();
			becameVisible = new CancellationTokenSource();
			
			OnEnabledAsync(becameVisible.Token)
				.ContinueWith(OnViewUpdated)
				.Forget();
		}

		public void Disable()
		{
			if(selfContainer && selfContainer.gameObject)
				SetActive(selfContainer, false);
			
			becameVisible?.Cancel();
			becameVisible?.Dispose();
			becameVisible = null;
			OnDisabled();
		}

		public override void Initialize()
		{
			//repeat base without setup activity view
			SetDynamicallyCreated(false);
			AutoInit();
			OnInit();
		}

		public override string ToString()
		{
			return RuntimeGameObject?.Data.Title ?? selfContainer.name;
		}

		protected virtual void OnSetup(){}

		protected virtual UniTask OnEnabledAsync(CancellationToken token)
		{
			return UniTask.CompletedTask;
		}

		protected virtual void OnDisabled(){}

		private void OnViewUpdated()
		{
			if(selfContainer && selfContainer.gameObject)
				SetActive(selfContainer, true);
		}
	}
}