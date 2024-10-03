using RR.Game.TutorialSystemV2.Abstraction;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Realizations
{

	public class DefaultTutorialCameraProvider : ITutorialCameraProvider
	{
		private Camera cached;
		public Camera Get()
		{
			return cached ? cached : cached = Camera.main;
		}
	}

}