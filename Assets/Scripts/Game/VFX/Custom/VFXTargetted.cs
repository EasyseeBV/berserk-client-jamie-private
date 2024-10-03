using System;
using UnityEngine;

namespace Vulcan.VFX.Custom
{
	public class VFXTargetted : VFXEntity
	{
		[SerializeField] private float speed = 10;
		private Vector3 target;
		private bool done;

		public override VFXEntity SetTargetPosition(Vector3 targetPosition)
		{
			target = SetOffset(targetPosition, true);
			return base.SetTargetPosition(targetPosition);
		}

		public override void Play(Func<bool> shouldDespawn = null)
		{
			base.Play(shouldDespawn);
			done = false;
		}

		private void Update()
		{
			if (done)
			{
				return;
			}

			if (transform.position == target)
			{
				done = true;
				OnComplete();
				return;
			}

			transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
		}
	}
}