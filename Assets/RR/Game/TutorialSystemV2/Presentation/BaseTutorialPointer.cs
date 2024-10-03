using System.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Presentation
{
	public abstract class BaseTutorialPointer : MonoBehaviour, 
		ITutorialInvokeHandler, ITutorialCloseHandler, ITutorialRemoveHandler, ITutorialOrderable
	{
		public int Order => 11;
		
		private void Start()
		{
			TutorialAdapter.HandlersRepository.Register(this);
			Init();
		}

		public virtual async Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (hint?.Pointer == null)
			{
				await CloseViewAsync();
				ResetToDefault();
				return;
			}

			ResetToDefault();
			SetPosition(hint.Pointer.StartPosition, hint.Pointer.EndPosition);
			await ShowViewAsync();
		}

		public virtual async Task CloseAsync(ITutorialHintEntity hint)
		{
			if (hint?.Pointer == null)
				return;
			
			await CloseViewAsync();
			ResetToDefault();
		}

		public virtual async Task RemovedAsync()
		{
			await CloseViewAsync();
			ResetToDefault();
			UnInit();
		}

		protected abstract void Init();

		protected abstract void UnInit();

		protected abstract Task ShowViewAsync();

		protected abstract Task CloseViewAsync();

		protected abstract void ResetToDefault();

		protected abstract void SetPosition(Vector3 startPosition, Vector3 endPosition);

		protected virtual void OnDestroy()
		{
			TutorialAdapter.HandlersRepository.UnRegister(this);
		}
	}
}