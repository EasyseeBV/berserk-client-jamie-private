using RR.Core.Components;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem.Animations;
using RR.UI.FrameSystem.Utils;
using RR.UI.Predefined;
using RR.UI.Predefined.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RR.Core.EventLayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RR.UI.FrameSystem
{
	/// <summary>UIManager controls opening and closing of views on scene</summary>
	[DefaultExecutionOrder(-1050)]
	public partial class UIManager : Singleton<UIManager>
	{
		public static bool IsUIBlockInteractions => BlockingInterationStack.Any(x => x != null);

#pragma warning disable 0649

		[Tooltip("Play ShowAnimation when view already shown")]
		[SerializeField] bool ReplayShowAnimation = false;
		[Tooltip("Dynamically created Views will be destroyed after closing")]
		[SerializeField] bool DestroyDynamicViews = true;
		[Tooltip("The Back closes App if back stack is empty")]
		[SerializeField] bool CloseAppOnBack = false;
		[Tooltip("Default gesture to Back")]
		[SerializeField] public Gesture BackGesture = Gesture.SwipeLeft;
		[SerializeField] bool CancelCausesBack = true;

		[Header("Default Prefabs")]
		[Tooltip("Prefab for fullscreen fade")]
		[SerializeField] FullscreenFade FullscreenFadePrefab = default;
		[Tooltip("Prefab for Dialog Window")]
		[SerializeField] DialogWindow DialogWindowPrefab = default;
		[Tooltip("Prefab for Dialog Input")]
		[SerializeField] DialogInput DialogInputPrefab = default;
		[Tooltip("Prefab for Background Sounds")]
		[SerializeField] AudioSource BackgroundSoundPrefab = default;
		[Tooltip("Prefab for Overlay Canvas")]
		[SerializeField] Canvas OverlayCanvasPrefab = default;

		[Header("Default Animations")]
		[SerializeField] AnimationLink ShowAnimation = default;
		[SerializeField] AnimationLink CloseAnimation = default;
		[SerializeField] AnimationLink HideAnimation = default;

		[Header("Default Sounds")]
		[SerializeField] public AudioClip ButtonSound = default;
		[SerializeField] public AudioClip BackSwipeSound = default;
		[SerializeField] public AudioClip OverlaySound = default;
		[SerializeField] float ButtonsVolume = 0.6f;
		[SerializeField] float FadeVolume = 0.25f;
		[SerializeField] float VolumeChangeSpeed = 1f;

		public static Transform Transform => Instance.transform;

		public static Dictionary<string, BaseView> StaticViews { get; } = new Dictionary<string, BaseView>();
		public static LinkedList<BaseView> BackStack { get; } = new LinkedList<BaseView>();
		public static List<BaseView> BlockingInterationStack { get; } = new List<BaseView>();

		public static State<bool> BackState = new State<bool>();

		private static Queue<Action> lateUpdateQueue = new Queue<Action>();

		public static Dragger Dragger = default;

		private SimpleGestures gestures = default;
		private AudioSource audioSource = default;
		private SoundManager soundManager = default;
		private float volumeMultiplier = 1;

		private string logModalErrorThresholdAccumulated;
		private string logModalWarningThresholdAccumulated;

#pragma warning restore 0649
		protected override void OnAwake()
		{
			audioSource = GetComponent<AudioSource>();

			StaticViews.Clear();
			BackStack.Clear();
			BlockingInterationStack.Clear();

			Dragger = new Dragger();
			soundManager = new SoundManager(this, BackgroundSoundPrefab, FadeVolume, VolumeChangeSpeed);

			ViewShown += soundManager.OnViewShown;
			ViewClosed += soundManager.OnViewClosed;

			InitGestures();
			SubscribeOnLoggerEvents();

			SceneManager.sceneLoaded += (x, y) => InitViewsOnScene();
		}

		private void SubscribeOnLoggerEvents()
		{
			RRLogger.OnError += x =>
			{
				if (!RRLogger.Options.ShowErrorsAsModalView)
					return;

				if (!string.IsNullOrEmpty(logModalErrorThresholdAccumulated))
				{
					logModalErrorThresholdAccumulated += $"\n---------- End Of Trace\n{x}";
					return;
				}

				logModalErrorThresholdAccumulated = x;
				StartCoroutine(LogModalErrorThresholdRoutine());
			};

			RRLogger.OnWarning += x =>
			{
				if (!RRLogger.Options.ShowWarningAsModalView)
					return;

				if (!string.IsNullOrEmpty(logModalWarningThresholdAccumulated))
				{
					logModalWarningThresholdAccumulated += $"\n---------- End Of Trace\n{x}";
					return;
				}

				logModalWarningThresholdAccumulated = x;
				StartCoroutine(LogModalWarningThresholdRoutine());
			};
		}

		private void InitViewsOnScene()
		{
			//find all views
			var views = SceneInfoGrabber<BaseView>
				.GetUIComponentsOnScene()
				.ToList();

			//grab components for views
			foreach (var view in views)
			{
				view.GrabComponents();
				StaticViews[view.GetType().Name] = view;
			}

			//grab views for views
			foreach (var view in views)
			{
				view.GrabViews(StaticViews);
			}

			//init views
			foreach (var view in views)
				view.Initialize();

			//show views
			foreach (var view in views.Where(v => v.ShowAtStart))
				Show(view, view.Owner, noAnimation: true);
		}

		private IEnumerator LogModalErrorThresholdRoutine()
		{
			yield return null;
			yield return null;

			ShowErrorDialog(logModalErrorThresholdAccumulated);
			logModalErrorThresholdAccumulated = null;
		}

		/// <summary>
		/// Shows formatted error dialog.
		/// </summary>
		public static void ShowErrorDialog(string message)
		{
			if (message.Length > 360)
				ShowDialog(default, $"{"Error!".Red().Bold()}\nCopy & send the stacktrace below to Support.", longMessage: message);
			else
				ShowDialog(default, message);
		}

		/// <summary>
		/// Shows formatted warning dialog.
		/// </summary>
		public static void ShowWarningDialog(string message)
		{
			if (message.Length > 360)
				ShowDialog(default, $"{"Warning!".Yellow().Bold()}", longMessage: message);
			else
				ShowDialog(default, message);
		}

		private IEnumerator LogModalWarningThresholdRoutine()
		{
			yield return null;
			yield return null;

			ShowWarningDialog(logModalWarningThresholdAccumulated);
			logModalWarningThresholdAccumulated = null;
		}

		bool cancelAxisFired;

		private void Update()
		{
			cancelAxisFired = !Mathf.Approximately(Input.GetAxis("Cancel"), 0);

			//Cancel => Back
			if (!cancelAxisFired)
				return;

			if (!cancelAxisFired && CancelCausesBack && Back())
				PlayOneShotSound(BackSwipeSound, 0.6f);
		}

		private void LateUpdate()
		{
			//execute late update queue
			while (lateUpdateQueue.Count > 0)
				lateUpdateQueue.Dequeue().Invoke();
		}

		#region Show/Close/Hide/Back

		/// <summary>Show the view</summary>
		public static void Show(BaseView view, BaseView owner, Action onAnimationDone = null, bool noAnimation = false, bool closeChildrenIfOpened = false, bool concurrentAnimation = false)
		{
			if (owner == view)
			{
				RRLogger.Error("Owner cant be same view, set to NULL");
				owner = null;
			}

			//close opened children
			if (closeChildrenIfOpened)
				CloseAllChildren(view);

			//add view to list of children of owner
			AddOpenedChildIfNotPresented(owner, view);

			//already opened?
			if (view.VisibleState == VisibleState.Visible)
				if (!Instance.ReplayShowAnimation)
				{
					onAnimationDone?.Invoke();
					return;
				}

			if (view.IsBlockingInteration && !BlockingInterationStack.Contains(view))
				BlockingInterationStack.Add(view);

			//hidden?
			if (view.VisibleState == VisibleState.Hidden)
			{
				ReopenHidden(view, onAnimationDone);
				return;
			}

			//close concurrent
			if (view.Concurrent)
				GetConcurrentList(view)
					.Where(x => x != view && x.VisibleState != VisibleState.Closed)
					.ForEach(x => Close(x));

			//todo: 2936 more concurrent types

			//AddToBackStack
			if (view.BackPrority != BackPrority.IgnoreBack)
			{
				AddToBackStack(view);
				BackState += true;
			}

			//hide owner
			if (view.HideOwner && view.Owner != null)
				Hide(view.Owner);

			//set visible state
			view.gameObject.SetActive(true);
			// Unity BUG: Layout wont recalculate without forcing, if a parent has layoutGroup...
			if (view.transform.parent is RectTransform rcParent)
				LayoutRebuilder.ForceRebuildLayoutImmediate(rcParent);

			OnViewShown(view);

			//get show animation
			var anim = view.Animations.ShowAnimation ?? Instance.ShowAnimation;

			//play show animation
			if (anim == null || anim.Animation == null || noAnimation)
				onAnimationDone?.Invoke();
			else
			{
				if (!concurrentAnimation)
					view.CurrentAnimation?.CurrentAnimation?.StopAndResetTransform();
				view.CurrentAnimation = AnimationPlayer.Play(view.RectTransform, anim.Animation, onAnimationDone, true, 1, 1);
			}

			//show FullscreenFade
			if (view.FullscreenFade && Instance.FullscreenFadePrefab != null)
			{
				ShowFullscreenFade(view);
			}
		}
		/// <summary>Show the fullscreen fade</summary>
		public static FullscreenFade ShowFullscreenFade(BaseView view)
		{
			var fade = Instantiate(Instance.FullscreenFadePrefab, view.transform.parent);
			var i = view.transform.GetSiblingIndex();
			fade.transform.SetSiblingIndex(i);
			fade.SetDynamicallyCreated(true);
			Show(fade, view);
			return fade;
		}
		/// <summary>Close the view and all children views</summary>
		public static void Close(BaseView view, Action onAnimationDone = null, bool noAnimation = false, bool concurrentAnimation = false)
		{
			//already closed?
			if (view.VisibleState == VisibleState.Closed)
			{
				//view.gameObject.SetActive(false);
				onAnimationDone?.Invoke();
				ClearOwnerAndShowIfHidden(view);
				return;
			}

			BlockingInterationStack.Remove(view);

			//close children w/o animation
			CloseAllChildren(view);

			//remove from back stack
			RemoveFromBackStack(view);

			//if was hidden - return
			if (view.VisibleState == VisibleState.Hidden)
			{
				view.gameObject.SetActive(false);
				ClearOwnerAndShowIfHidden(view);
				return;
			}

			//get close animation
			var anim = view.Animations.CloseAnimation ?? Instance.CloseAnimation;

			if (noAnimation == true || anim == null || anim.Animation == null)
				OnAnimationDone();
			else
			{
				if (!concurrentAnimation)
					view.CurrentAnimation?.CurrentAnimation?.StopAndResetTransform();
				view.CurrentAnimation = AnimationPlayer.Play(view.RectTransform, anim.Animation, OnAnimationDone, true, 1, 1);
			}

			ClearOwnerAndShowIfHidden(view);

			//play animation and deactivate
			void OnAnimationDone()
			{
				if (view)
				{
					view.gameObject.SetActive(false);
					//close
					OnViewClosed(view);
					if (Instance.DestroyDynamicViews && view.IsDynamicallyCreated)
						Destroy(view.gameObject);
				}

				onAnimationDone?.Invoke();
			}
		}

		/// <summary>Make the view invisible, but do not close children</summary>
		public static void Hide(BaseView view, Action onAnimationDone = null, bool noAnimation = false, bool concurrentAnimation = false)
		{
			//already hidden?
			if (view.VisibleState == VisibleState.Hidden)
			{
				view.gameObject.SetActive(false);
				onAnimationDone?.Invoke();
				return;
			}

			//hide
			OnViewHidden(view);

			BlockingInterationStack.Remove(view);

			void OnAnimationDone()
			{
				view.gameObject.SetActive(false);
				onAnimationDone?.Invoke();
			}

			var anim = view.Animations?.HideAnimation ?? Instance.HideAnimation;
			if (noAnimation || anim == null || anim.Animation == null)
			{
				OnAnimationDone();
				return;
			}

			if (!concurrentAnimation)
				view.CurrentAnimation?.CurrentAnimation?.StopAndResetTransform();
			view.CurrentAnimation = AnimationPlayer.Play(view.RectTransform, anim.Animation, OnAnimationDone, true, 1, 1);
		}

		/// <summary>Show the view if closed or hidden, close otherwise</summary>
		public static void ShowOrClose(BaseView view, BaseView owner, Action onAnimationDone = null,
			bool noAnimation = false, bool concurrentAnimation = false)
		{
			switch (view.VisibleState)
			{
				case VisibleState.Visible: Close(view, onAnimationDone, noAnimation, concurrentAnimation); break;
				case VisibleState.Hidden: ReopenHidden(view, onAnimationDone, noAnimation, concurrentAnimation); break;
				case VisibleState.Closed: Show(view, owner, onAnimationDone, noAnimation, false, concurrentAnimation); break;
			}
		}

		/// <summary>Close last view in Back stack</summary>
		public static bool Back(Action onAnimationDone = null)
		{
			if (BackStack.Count == 0)
			{
				if (!Instance.CloseAppOnBack)
					return false;

				Application.Quit();
				return true;
			}

			//get last and remove from back stack
			var view = BackStack.Last.Value;
			if (view.SuppressBack)
				return false;//ignore back for SuppressBack view

			view.Back(onAnimationDone);

			return true;
		}

		/// <summary>Closes all children</summary>
		public static void CloseAllChildren(BaseView view)
		{
			//close children w/o anitmation
			foreach (var child in view.OpenedChildren.ToArray())
				CloseNoAnimation(child);

			if (view.VisibleState == VisibleState.Hidden)
				ReopenHidden(view);
		}

		/// <summary>Close all children and show the view with previous Owner</summary>
		public static void Reopen(BaseView view, Action onAnimationDone = null, bool noAnimation = false)
		{
			Show(view, view.Owner, onAnimationDone, noAnimation, true);
		}

		private static void ClearOwnerAndShowIfHidden(BaseView view)
		{
			var owner = view.Owner;

			//remove from opened children list
			owner?.OpenedChildren.Remove(view);
			view.Owner = null;

			//reopen hidden owner
			if (view.HideOwner)
				if (owner != null && owner.VisibleState == VisibleState.Hidden)
				{
					view = owner;
					ReopenHidden(view);
				}
		}

		private static void ReopenHidden(BaseView view, Action onAnimationDone = null, bool noAnimation = false, bool concurrentAnimation = false)
		{
			//set visible state
			view.gameObject.SetActive(true);
			OnViewShown(view);

			//get show animation
			var anim = view.Animations.ShowAnimation ?? Instance.ShowAnimation;

			//play show animation
			if (anim == null || anim.Animation == null)
			{
				onAnimationDone?.Invoke();
				return;
			}

			if (noAnimation)
				return;

			if (!concurrentAnimation)
				view.CurrentAnimation?.CurrentAnimation?.StopAndResetTransform();
			view.CurrentAnimation = AnimationPlayer.Play(view.RectTransform, anim.Animation, onAnimationDone, true, 1, 1);
		}

		private static void CloseNoAnimation(BaseView view)
		{
			if (view == null) throw new ArgumentNullException(nameof(view));

			if (view.VisibleState == VisibleState.Closed)
				return;

			//close children
			foreach (var child in view.OpenedChildren.ToArray())
				CloseNoAnimation(child);

			RemoveFromBackStack(view);

			//close
			view.gameObject.SetActive(false);

			OnViewClosed(view);

			// FIXME Above
			if (Instance.DestroyDynamicViews && view.IsDynamicallyCreated)
				Destroy(view.gameObject);

			//remove from owner list
			view.Owner?.OpenedChildren.Remove(view);
		}

		private static void RemoveFromBackStack(BaseView view)
		{
			if (view.BackPrority == BackPrority.IgnoreBack)
				return;

			while (BackStack.Remove(view)) ;
			BackState += false;
		}

		private static IEnumerable<BaseView> GetConcurrentList(BaseView view)
		{
			var parent = view.RectTransform.parent;
			return parent.OfType<RectTransform>().Select(rt => rt.GetComponent<BaseView>()).Where(v => v != null);
		}

		#endregion

		#region Events

		public static event Action<BaseView> ViewShown;
		public static event Action<BaseView> ViewClosed;
		public static event Action<BaseView> ViewHidden;
		public static event Action<BaseView, VisibleState, VisibleState> ViewVisibleStateChanged;

		public static readonly List<BaseView> ShownViews = new List<BaseView>();

		private static void OnViewShown(BaseView view)
		{
			var prevState = view.VisibleState;

			(view as IBaseViewInternal).SetVisibleState(VisibleState.Visible);

			ViewShown?.Invoke(view);
			ViewVisibleStateChanged?.Invoke(view, prevState, view.VisibleState);
			if (!ShownViews.Contains(view))
				ShownViews.Add(view);
		}

		private static void OnViewClosed(BaseView view)
		{
			var prevState = view.VisibleState;

			(view as IBaseViewInternal).SetVisibleState(VisibleState.Closed);

			ViewClosed?.Invoke(view);
			ViewVisibleStateChanged?.Invoke(view, prevState, view.VisibleState);
			ShownViews.Remove(view);
		}

		private static void OnViewHidden(BaseView view)
		{
			var prevState = view.VisibleState;

			(view as IBaseViewInternal).SetVisibleState(VisibleState.Hidden);

			ViewHidden?.Invoke(view);
			ViewVisibleStateChanged?.Invoke(view, prevState, view.VisibleState);
			ShownViews.Remove(view);
		}

		#endregion

		#region ShowCoroutine, ShowAsync

		/// <summary>Shows the view and wait while it will be closed</summary>
		public static IEnumerator ShowCoroutine(BaseView view, BaseView owner, Action onAnimationDone = null, bool noAnimation = false, bool closeChildrenIfOpened = false)
		{
			Show(view, owner, onAnimationDone, noAnimation, closeChildrenIfOpened);
			while (view.VisibleState != VisibleState.Closed)
				yield return null;
		}

		/// <summary>Shows the view and wait while it will be closed</summary>
		public static async Task ShowAsync(BaseView view, BaseView owner, Action onAnimationDone = null, bool noAnimation = false, bool closeChildrenIfOpened = false)
		{
			Show(view, owner, onAnimationDone, noAnimation, closeChildrenIfOpened);

			// TODO: 2918 Remove TaskEx - replace with states
			await TaskEx.WaitWhile(() => view.VisibleState != VisibleState.Closed);
		}

		#endregion

		#region Gestures

		private void InitGestures()
		{
			//init gestures
			gestures = GetComponent<SimpleGestures>();
			gestures.onSwipeHoriz += (i) => ProcessGesture(i < 0 ? Gesture.SwipeLeft : Gesture.SwipeRight);
			gestures.onSwipeVert += (i) => ProcessGesture(i < 0 ? Gesture.SwipeDown : Gesture.SwipeUp);
			gestures.onSwipeFromHoriz += (i, p) =>
			{
				if (i > 0 && p < 0.1f * Screen.width)
				{
					ProcessGesture(Gesture.SwipeFromLeft);
				}
				else if (i > 0 && p < 0.9f * Screen.width)
				{
					ProcessGesture(Gesture.SwipeFromRight);
				}
			};
			gestures.onSwipeFromVert += (i, p) =>
			{
				if (i > 0 && p < 0.1f * Screen.height)
				{
					ProcessGesture(Gesture.SwipeFromUp);
				}
				else if (i > 0 && p < 0.9f * Screen.height)
				{
					ProcessGesture(Gesture.SwipeFromDown);
				}
			};
			gestures.onTap += (i) => ProcessGesture(Gesture.Tap);
			gestures.onLongTap += (i) => ProcessGesture(Gesture.LongTap);
			gestures.onDoubleTap += (i) => ProcessGesture(Gesture.DoubleTap);
			gestures.onPan += (v) => { if (Dragger.IsDragging) Dragger.OnDragging(v); };
			gestures.onDragStart += (v) => Dragger.OnDragStart(v, gestures.LastTouchedUI);
			gestures.onDragEnd += Dragger.OnDragEnd;
		}

		private void ProcessGesture(Gesture gest)
		{
			if (Dragger.IsDragging)
				return;//we are in drag mode => ignore gestures

			//get last touched UI
			var ui = gestures.LastTouchedUI;
			if (ui == null)
			{
				DefaultGestureProcessing(gest);
				return;
			}

			//find touched BaseView
			var view = ui.GetComponentsInParent<BaseView>().FirstOrDefault(v => v.VisibleState != VisibleState.Closed);

			//try process gesture in touched view and it's owners
			var info = new GestureInfo(gest, view);
			while (view != null)
			{
				//call method of view
				view.OnGesture(info);
				if (info.IsHandled || view.SuppressAnyGesturesForOwners)
					return;//gesture is handled
						   //go to owner
				view = view.Owner;
			}

			//gesture is not handled => try process Back
			DefaultGestureProcessing(gest);
		}

		private void DefaultGestureProcessing(Gesture gest)
		{
			//is Back gesture?
			if (gest == BackGesture && BackStack.Count > 0)
			{
				PlayOneShotSound(BackSwipeSound, 0.6f);
				Back();
			}
		}

		#endregion

		#region Dialog Windows

		public static DialogWindow ShowDialog(BaseView owner, string message, string okText = "Ok", string cancelText = null, bool closeByTap = false, Action<DialogResult> onClosed = null, string longMessage = null)
		{
			var canvas = Instantiate(Instance.OverlayCanvasPrefab);
			var view = Instantiate(Instance.DialogWindowPrefab, canvas.transform);
			if (onClosed != null)
				view.Closed += () => onClosed(view.DialogResult);

			view.Closed += () => Destroy(canvas.gameObject);
			view.Build(message, okText, cancelText, closeByTap, longMessage);
			view.Show(owner);
			return view;
		}

		public static async Task<DialogResult> ShowDialogAsync(BaseView owner, string message, string okText = "Ok", string cancelText = null, bool closeByTap = false, string longMessage = null)
		{
			var canvas = Instantiate(Instance.OverlayCanvasPrefab);
			var view = Instantiate(Instance.DialogWindowPrefab, canvas.transform);
			view.GrabComponents();
			view.Initialize();
			view.gameObject.SetActive(false);
			view.Build(message, okText, cancelText, closeByTap, longMessage);
			await view.ShowAsync(owner);
			Destroy(canvas.gameObject, 1);
			return view.DialogResult;
		}

		public static IEnumerator ShowDialogCoroutine(BaseView owner = default, string message = null, string okText = "Ok", string cancelText = null, bool closeByTap = false, Action<DialogResult> onClosed = null, string longMessage = null)
		{
			var view = Instantiate(Instance.DialogWindowPrefab);
			view.Build(message, okText, cancelText, closeByTap, longMessage);
			yield return view.ShowCoroutine(owner);
			onClosed?.Invoke(view.DialogResult);
		}

		public static DialogInput ShowDialogInput(BaseView owner = default, string message = null, string placeHolderText = "", string okText = "OK", string cancelText = "Cancel", Action<string> onClosed = null)
		{
			var canvas = Instantiate(Instance.OverlayCanvasPrefab);
			var view = Instantiate(Instance.DialogInputPrefab, canvas.transform);
			if (onClosed != null)
				view.Closed += () => onClosed(view.Result);

			view.Build(message, okText, cancelText, placeHolderText);
			view.Show(owner);
			return view;
		}

		public static async Task<string> ShowDialogInputAsync(BaseView owner = null, string message = null, string placeHolderText = "", string okText = "OK", string cancelText = "Cancel")
		{
			var canvas = Instantiate(Instance.OverlayCanvasPrefab);
			var view = Instantiate(Instance.DialogInputPrefab, canvas.transform);

			view.GrabComponents();
			view.Initialize();
			view.gameObject.SetActive(false);
			view.Build(message, okText, cancelText, placeHolderText);

			await view.ShowAsync(owner);

			Destroy(canvas.gameObject, 1);

			return view.Result;
		}

		public static IEnumerator ShowDialogInputCoroutine(BaseView owner, string message, string text, string placeHolderText = "", string okText = "OK", string cancelText = "Cancel", Action<string> onClosed = null)
		{
			var canvas = Instantiate(Instance.OverlayCanvasPrefab);
			var view = Instantiate(Instance.DialogInputPrefab, canvas.transform);
			view.Build(message, okText, cancelText, placeHolderText);

			yield return view.ShowCoroutine(owner);

			Destroy(canvas.gameObject, 1);
			onClosed?.Invoke(view.Result);
		}

		#endregion

		#region Sounds

		public static void PlayButtonSound(BaseView view)
		{
			if (Instance.ButtonSound)
				PlayOneShotSound(Instance.ButtonSound, Instance.ButtonsVolume);
		}

		public static void PlayOverlaySound(BaseView view)
		{
			if (Instance.OverlaySound)
				PlayOneShotSound(Instance.OverlaySound, Instance.ButtonsVolume);
		}

		public static void PlayOneShotSound(AudioClip clip, float volume = 1f)
		{
			if (clip)
				Instance.audioSource.PlayOneShot(clip, volume * Instance.volumeMultiplier);
		}

		public static void SetAudioVolume(float volumeMult)
		{
			Instance.volumeMultiplier = volumeMult;
		}

		#endregion

		#region Utils

		private static void AddToBackStack(BaseView view)
		{
			//remove if presented
			while (BackStack.Remove(view)) ;

			//add to end
			if (BackStack.Count == 0)
			{
				BackStack.AddLast(view);
				return;
			}

			//find by priority
			var node = BackStack.Last;
			while (node != null && node.Value.BackPrority > view.BackPrority)
				node = node.Previous;

			if (node == null)
			{
				BackStack.AddFirst(view);
				return;
			}

			BackStack.AddAfter(node, view);
		}

		private static void AddOpenedChildIfNotPresented(BaseView owner, BaseView child)
		{
			//hide prev owner 
			if (child.Owner != owner)
				ClearOwnerAndShowIfHidden(child);

			child.Owner = owner;

			if (owner == null)
				return;

			if (!owner.OpenedChildren.Contains(child))
				owner.OpenedChildren.Add(child);
		}

		public static IEnumerable<BaseView> GetViewsUnderMouse()
		{
			var uis = SimpleGestures.GetUIObjectsUnderPosition(Input.mousePosition).Select(r => r.gameObject);
			foreach (var ui in uis)
			{
				var view = ui.GetComponentsInParent<BaseView>().FirstOrDefault(v => v.VisibleState != VisibleState.Closed);
				while (view != null)
				{
					yield return view;
					view = view.Owner;
				}
			}
		}

		public static void AddLateUpdateAction(Action act)
		{
			lateUpdateQueue.Enqueue(act);
		}

		#endregion

#if UNITY_EDITOR
		private void Reset()
		{
			name = "UI Manager";
			if (GetComponent<Canvas>() != null)
				name += " [Canvas]";
		}
#endif
	}

	public enum Gesture
	{
		None, SwipeLeft, SwipeRight, SwipeUp, SwipeDown, Tap, LongTap, DoubleTap, SwipeFromLeft, SwipeFromRight, SwipeFromUp, SwipeFromDown
	}

	public class GestureInfo
	{
		/// <summary>Touched BaseView</summary>
		public BaseView TouchedUI { get; }
		/// <summary>Gesture type</summary>
		public Gesture Gesture { get; }
		/// <summary>Set to True to prevent owner to process gesture</summary>
		public bool IsHandled { get; set; }

		public GestureInfo(Gesture gesture, BaseView touchedView)
		{
			Gesture = gesture;
			TouchedUI = touchedView;
		}
	}
}