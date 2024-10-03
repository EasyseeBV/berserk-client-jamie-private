using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RR.Game.ParallaxSystem
{
	public class Parallax : MonoBehaviour
	{
		public delegate void ParallaxCameraDelegate(float deltaX, float deltaY);
		public ParallaxCameraDelegate OnCameraTranslate;

		[SerializeField] private List<ParallaxElement> elements;

		[Header("Stats")]
		[SerializeField] private float speed = 1.0f;
		[SerializeField] private bool childsOnly = true;

		private float oldPositionX, oldPositionY;
		private Transform cam;

		public void AddElement(ParallaxElement element)
		{
			elements.Add(element);
			OnCameraTranslate += element.Move;
		}

		private void Awake()
		{
			if (childsOnly)
				elements = GetComponentsInChildren<ParallaxElement>(true).ToList();

			elements.ForEach(x => OnCameraTranslate += x.Move);
			cam = Camera.main.transform;
		}

		private void LateUpdate()
		{
			if (Math.Abs(cam.position.x - oldPositionX) < Mathf.Epsilon 
			    && Math.Abs(cam.position.y - oldPositionY) < Mathf.Epsilon)
				return;

			if (OnCameraTranslate != null)
			{
				var delta1 = oldPositionX - cam.position.x;
				var delta2 = oldPositionY - cam.position.y;

				OnCameraTranslate(delta1 * speed, delta2 * speed);
			}

			oldPositionX = cam.position.x;
			oldPositionY = cam.position.y;
		}
	}
}