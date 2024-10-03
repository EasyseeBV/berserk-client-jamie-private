using System;
using System.Collections;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual
{
	public class VFXProjectile : VFXView
	{
		[SerializeField] private float speed = 10;

		private bool isSetupDone;
		private Vector3 from;
		private Vector3 to;

		private void OnEnable()
		{
			StartCoroutine(MoveRoutine());
		}

		public override void SetArguments(params object[] args)
		{
			if (args == null || args.Length != 2)
				throw new Exception("Arguments should contain 2 elements");

			from = (args[0] as Transform).position;
			to = (args[1] as Transform).position;
			
			transform.position = from;
			isSetupDone = true;
		}

		private IEnumerator MoveRoutine()
		{
			while (!isSetupDone)
				yield return null;

			Vector3 delta;
			var t = transform;

			do
			{
				yield return null;

				t.position = Vector3.MoveTowards(t.position, to, speed * Time.deltaTime);
				delta = to - t.position;
			} while (delta.magnitude > .1f);

			DestroyInstance();
		}
	}
}