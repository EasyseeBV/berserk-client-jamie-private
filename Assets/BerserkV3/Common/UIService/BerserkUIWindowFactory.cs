using System;
using System.Linq;
using RR.UIService;
using RR.UIService.AnimationSource;
using RR.UIService.AudioSource;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BerserkV3.Common.UIService
{
	public class BerserkUIWindowFactory : IUIWindowFactory
	{
		private readonly IInstantiator instantiator;
		private readonly IUIPrototypeProvider prototypeProvider;
		private readonly IUIAudioSourceFactory audioSourceFactory;
		private readonly IUIAnimationSourceFactory animationSourceFactory;

		public BerserkUIWindowFactory(
			IInstantiator instantiator,
			IUIPrototypeProvider prototypeProvider,
			IUIAudioSourceFactory audioSourceFactory,
			IUIAnimationSourceFactory animationSourceFactory)
		{
			this.instantiator = instantiator;
			this.prototypeProvider = prototypeProvider;
			this.audioSourceFactory = audioSourceFactory;
			this.animationSourceFactory = animationSourceFactory;
		}

		public T Create<T>(string groupId, Transform parent = null) where T : IUIWindow
			=> Create(prototypeProvider.Get(groupId, typeof(T)), parent).GetComponent<T>();

		public IUIWindow[] CreateAll(Transform parent = null)
			=> prototypeProvider.GetAll().Select(prototype => Create(prototype, parent)).ToArray<IUIWindow>();

		public IUIWindow[] Create(string groupId, Transform parent = null)
			=> prototypeProvider.GetAll(groupId).Select(prototype => Create(prototype, parent)).ToArray<IUIWindow>();

		private UIWindowBase Create(Object prototype, Transform parent = null) 
			=> Initialize(instantiator.InstantiatePrefabForComponent<UIWindowBase>(prototype, parent));

		private UIWindowBase Initialize(UIWindowBase window)
		{
			var animationSource = animationSourceFactory.Create(window);
			var audioSource = audioSourceFactory.Create(window);
			return window.Init(Guid.NewGuid().ToString(), animationSource, audioSource);
		}
	}
}