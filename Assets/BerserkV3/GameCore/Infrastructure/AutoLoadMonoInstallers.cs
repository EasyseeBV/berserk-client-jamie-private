using System;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	[RequireComponent(typeof(RunnableContext))]
	public class AutoLoadMonoInstallers : MonoBehaviour
	{
		private void Awake()
		{
			if (!TryGetComponent(out RunnableContext context))
				throw new NullReferenceException($"Set in the inspector the {nameof(RunnableContext)}");

			if (context.Initialized)
				throw new InvalidOperationException($"Context already initialized! Check the {nameof(RunnableContext)} autorun is disabled.");
			
			context.Installers = GetComponentsInChildren<MonoInstaller>();
			context.Run(); // Check the RunnableContext autorun is disabled.
		}
	}
}