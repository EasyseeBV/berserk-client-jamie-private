using System.Collections.Generic;
using UnityEngine;

namespace BerserkV3.Generic.Emotions
{
	public class EmotionsViewFactory : IEmotionsViewFactory
	{
		private readonly Dictionary<string, GameObject> prefabs;
		
		public EmotionsViewFactory()
		{
			prefabs = new Dictionary<string, GameObject>();
		}

		public IEmotionView Create(string name, Transform parent = null)
		{
			return Object.Instantiate(GePrefab(name), parent).GetComponent<IEmotionView>();
		}

		private GameObject GePrefab(string name)
		{
			if (prefabs.TryGetValue(name, out var prefab)) 
				return prefab;
			
			prefabs.Add(name, Resources.Load<GameObject>(name));
			return prefabs[name];
		}
	}
}