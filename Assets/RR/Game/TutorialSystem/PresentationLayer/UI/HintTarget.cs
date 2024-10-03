using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace RR.Game.TutorialSystem.Presentation.UI
{
	/// <summary>
	///     Add this component to the element UI that will be selected during the tutor hint
	/// </summary>
	public abstract class HintTarget : MonoBehaviour
	{
		[Tooltip("Unique ID of the tutorial hint, as in the config")]
		[SerializeField] private string tutorId;

		[Tooltip(
			"GameObjects or UI elements that are enabled while a tutorial hint is running and disabled when finished")]
		[SerializeField] private List<GameObject> gameObjectsToActivate = new();

		[Tooltip("GameObjects or UI elements that are disabled or enabled while the tooltip is running")]
		[SerializeField] private UnitySerializedDictionary<GameObject, bool> managedObjects = new();

		[Tooltip("Buttons that will not work during the tutorial hint")]
		[SerializeField] private Button[] notTutorButtons = Array.Empty<Button>();

		[Tooltip("Scroll that will not work during the tutorial hint")]
		[SerializeField] private ScrollRect[] notTutorScroll = Array.Empty<ScrollRect>();

		[Tooltip("Collider that will not work during the tutorial hint")]
		[SerializeField] private Collider[] notTutorColliders = Array.Empty<Collider>();

		[Tooltip("Colliders elements that are disabled or enabled while the tooltip is running")]
		[SerializeField] private UnitySerializedDictionary<Collider, bool> managedColliders = new();

		[Tooltip("`Behaviour` elements that are disabled or enabled while the tooltip is running")]
		[SerializeField] private UnitySerializedDictionary<Behaviour, bool> managedComponents = new();

		[Tooltip("GameObjects or UI elements that are disabled or enabled after the tooltip was closed")]
		[SerializeField] private UnitySerializedDictionary<GameObject, bool> managedObjectsOnClose = new();

		[Tooltip("Colliders elements that are disabled or enabled after the tooltip was closed")]
		[SerializeField] private UnitySerializedDictionary<Collider, bool> managedCollidersOnClose = new();

		[Tooltip("`Behaviour` elements that are disabled or enabled after the tooltip was closed")]
		[SerializeField] private UnitySerializedDictionary<Behaviour, bool> managedComponentsOnClose = new();

		[Tooltip("Buttons, after clicking on which the tutorial hint will close")]
		[SerializeField] protected Button[] TargetButtons;

		private List<Button> buttons = new();
		private TutorHintView tutorHintView;

		public string HintId => string.IsNullOrEmpty(tutorId)
			? gameObject.name
			: tutorId;

		public abstract bool IsClickToCloseHint { get; }

		private void Awake()
		{
			OnAwake();
		}

		private void Start()
		{
			if (TutorController.CanAddTutorElement(HintId))
			{
				TutorialUIPanel.Instance.AddTarget(this);
				RRLogger.Log(
					$"{nameof(TutorHintView)} with {nameof(HintId)} {HintId} add to {nameof(TutorialUIPanel)} success");
				return;
			}

			gameObjectsToActivate = null;
			managedObjects = null;
			managedColliders = null;
			managedComponents = null;
			managedObjectsOnClose = null;
			managedCollidersOnClose = null;
			managedComponentsOnClose = null;
			RRLogger.Log($"{nameof(TutorHintView)} with {nameof(HintId)} {HintId} remove from object {name}");
			Destroy(this);
		}

		protected virtual void OnAwake()
		{
		}

		public virtual void SetUp(TutorHintView tutorHintView)
		{
			this.tutorHintView = tutorHintView;
			if (TargetButtons is {Length: > 0})
			{
				//Bug #325yvet Button.Subscribe
				TargetButtons.ForEach(button => button.onClick.AddListener(OnTargetClick));
				buttons.AddRange(TargetButtons);
			}

			notTutorButtons.ForEach(x => x.interactable = false);
			notTutorScroll.ForEach(x =>
			{
				x.horizontal = false;
				x.vertical = false;
			});
			notTutorColliders.ForEach(x => x.enabled = false);

			foreach (var go in gameObjectsToActivate)
				go.SetActive(true);

			foreach (var managedObject in managedObjects)
				managedObject.Key.SetActive(managedObject.Value);

			foreach (var managedCollider in managedColliders)
				managedCollider.Key.enabled = managedCollider.Value;

			foreach (var managedComponent in managedComponents)
				managedComponent.Key.enabled = managedComponent.Value;

			tutorHintView.Closed += ResetToDefault;
		}

		public void SubscribeOnContinue(Button tutorContinueButton)
		{
			buttons.Add(tutorContinueButton);
			//Bug #325yvet Button.Subscribe
			tutorContinueButton.onClick.AddListener(OnTargetClick);
		}

		protected void OnTargetClick()
		{
			ResetToDefault();
			RRLogger.Log($"Click from {HintId} to close hint. {nameof(IsClickToCloseHint)} = {IsClickToCloseHint}");
			TutorController.CloseHintManually();
		}

		protected virtual void ResetToDefault()
		{
			buttons.ForEach(x => x.onClick.RemoveListener(OnTargetClick));
			buttons.Clear();

			notTutorButtons.ForEach(x => x.interactable = true);
			notTutorScroll.ForEach(x => x.horizontal = true);
			notTutorColliders.ForEach(x => x.enabled = true);

			if (gameObjectsToActivate != null)
				foreach (var go in gameObjectsToActivate)
					go.Value()?.SetActive(false);

			if (managedObjectsOnClose != null)
			{
				foreach (var managedObject in managedObjectsOnClose)
					managedObject.Key.SetActive(managedObject.Value);

				managedObjectsOnClose.Clear();
			}

			if (managedCollidersOnClose != null)
			{
				foreach (var (managedCollider, value) in managedCollidersOnClose)
					managedCollider.enabled = value;

				managedCollidersOnClose.Clear();
			}

			if (managedComponentsOnClose != null)
			{
				foreach (var (managedComponent, value) in managedComponentsOnClose)
					managedComponent.enabled = value;

				managedComponentsOnClose.Clear();
			}

			if (tutorHintView.Value() == null)
				return;

			tutorHintView.Closed -= ResetToDefault;
		}
	}
}