using System.Collections.Generic;
using System.Linq;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.TargetSystem;
using UnityEngine.InputSystem;
using Zenject;

namespace BerserkV3.Common.InputSystem
{
	public class InputSystemInstaller : Installer<InputSystemInstaller>
	{
		public override void InstallBindings()
		{
			BindInput();
			DragSystemInstaller.Install(Container);
			HoveringSystemInstaller.Install(Container);
			SelectionSystemInstaller.Install(Container);
		}

		private void BindInput()
		{
			var contextProcessor = new InputContextProcessor();
			var gamePlayInput = new GamePlayInput();
			gamePlayInput.Enable(); // before binding input actions
			
			Container
				.Bind<GamePlayInput>()
				.FromInstance(gamePlayInput)
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<InputContextProcessor>()
				.FromInstance(contextProcessor)
				.AsSingle()
				.NonLazy();

			BindInputController<DragSystemActions>(gamePlayInput.DragSystem.Get(), contextProcessor);
			BindInputController<HoveringSystemActions>(gamePlayInput.HoveringSystem.Get(), contextProcessor);
			BindInputController<SelectionSystemActions>(gamePlayInput.SelectionSystem.Get(), contextProcessor);
			BindInputController<PreviewType>(gamePlayInput.PreviewSystem.Get(), contextProcessor);
			BindInputController<TargetSystemActions>(gamePlayInput.TargetSystem.Get(), contextProcessor);
		}

		private void BindInputController<TAction>(InputActionMap actionMap, IInputContextProcessor contextProcessor) where TAction : struct
		{
			// Generics in assembly with IL2CPP - runtime error, deterministic generation of Generics required.
			// https://forum.unity.com/threads/is-there-any-limitations-to-deserializing-json-on-webgl.1250356/#post-7951618
			var args = MapInputActions(actionMap, contextProcessor);
			var inputController = new InputController<TAction>(args);
			Container
				.BindInterfacesTo<InputController<TAction>>()
				.FromInstance(inputController)
				.AsSingle()
				.NonLazy();
		}

		private IEnumerable<IInputAction> MapInputActions(InputActionMap actionMap, IInputContextProcessor contextProcessor)
		{
			return actionMap.actions.Select(inputAction => new InputSystemInputAction(inputAction, contextProcessor));
		}
	}
}