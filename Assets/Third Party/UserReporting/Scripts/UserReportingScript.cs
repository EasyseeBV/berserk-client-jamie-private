using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Authorization.Inventory.Models;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using TMPro;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/// <summary>
///     Represents a behavior for working with the user reporting client.
/// </summary>
/// <remarks>
///     This script is provided as a sample and isn't necessarily the most optimal solution for your project.
///     You may want to consider replacing with this script with your own script in the future.
/// </remarks>
public class UserReportingScript : MonoBehaviour
{
	#region DI

	private IInventoryApplication userInventory; 
	
	[Inject]
	public void Construct(IInventoryApplication userInventory)
	{
		this.userInventory = userInventory;
	}

	#endregion
	
	#region Constructors

	/// <summary>
	///     Creates a new instance of the <see cref="UserReportingScript" /> class.
	/// </summary>
	private void Awake()
	{
		UserReportSubmitting = new UnityEvent();
		unityUserReportingUpdater = new UnityUserReportingUpdater();
		if (SubmittingPopup)
			SubmittingPopup.SetActive(false);
		
		if (ErrorPopup)
			ErrorPopup.SetActive(false);
		
		if (UserReportForm)
			UserReportForm.SetActive(false);
	}

	#endregion

	#region Fields

	/// <summary>
	///     Gets or sets the category dropdown.
	/// </summary>
	[Tooltip("The category dropdown.")] public Dropdown CategoryDropdown;

	/// <summary>
	///     Gets or sets the description input on the user report form.
	/// </summary>
	[Tooltip("The description input on the user report form.")]
	public TMP_InputField DescriptionInput;

	/// <summary>
	///     Gets or sets the UI shown when there's an error.
	/// </summary>
	[Tooltip("The UI shown when there's an error.")]
	public GameObject ErrorPopup;

	private bool isCreatingUserReport;

	/// <summary>
	///     Gets or sets a value indicating whether the hotkey is enabled (Left Alt + Left Shift + B).
	/// </summary>
	[Tooltip("A value indicating whether the hotkey is enabled (Left Alt + Left Shift + B).")]
	public bool IsHotkeyEnabled;

	/// <summary>
	///     Gets or sets a value indicating whether the prefab is in silent mode. Silent mode does not show the user report
	///     form.
	/// </summary>
	[Tooltip(
		"A value indicating whether the prefab is in silent mode. Silent mode does not show the user report form.")]
	public bool IsInSilentMode;

	/// <summary>
	///     Gets or sets a value indicating whether the user report client reports metrics about itself.
	/// </summary>
	[Tooltip("A value indicating whether the user report client reports metrics about itself.")]
	public bool IsSelfReporting;

	private bool isShowingError;

	private bool isSubmitting;

	/// <summary>
	///     Gets or sets the display text for the progress text.
	/// </summary>
	[Tooltip("The display text for the progress text.")]
	public Text ProgressText;

	/// <summary>
	///     Gets or sets a value indicating whether the user report client send events to analytics.
	/// </summary>
	[Tooltip("A value indicating whether the user report client send events to analytics.")]
	public bool SendEventsToAnalytics;

	/// <summary>
	///     Gets or sets the UI shown while submitting.
	/// </summary>
	[Tooltip("The UI shown while submitting.")]
	public GameObject SubmittingPopup;

	/// <summary>
	///     Gets or sets the summary input on the user report form.
	/// </summary>
	[Tooltip("The summary input on the user report form.")]
	public TMP_InputField SummaryInput;

	/// <summary>
	///     Gets or sets the thumbnail viewer on the user report form.
	/// </summary>
	[Tooltip("The thumbnail viewer on the user report form.")]
	public Image ThumbnailViewer;

	private UnityUserReportingUpdater unityUserReportingUpdater;

	/// <summary>
	///     Gets or sets the user report button used to create a user report.
	/// </summary>
	[Tooltip("The user report button used to create a user report.")]
	public Button UserReportButton;

	/// <summary>
	///     Gets or sets the UI for the user report form. Shown after a user report is created.
	/// </summary>
	[Tooltip("The UI for the user report form. Shown after a user report is created.")]
	public GameObject UserReportForm;

	/// <summary>
	///     Gets or sets the User Reporting platform. Different platforms have different features but may require certain Unity
	///     versions or target platforms. The Async platform adds async screenshotting and report creation, but requires Unity
	///     2018.3 and above, the package manager version of Unity User Reporting, and a target platform that supports
	///     asynchronous GPU readback such as DirectX.
	/// </summary>
	[Tooltip(
		"The User Reporting platform. Different platforms have different features but may require certain Unity versions or target platforms. The Async platform adds async screenshotting and report creation, but requires Unity 2018.3 and above, the package manager version of Unity User Reporting, and a target platform that supports asynchronous GPU readback such as DirectX.")]
	public UserReportingPlatformType UserReportingPlatform;

	/// <summary>
	///     Gets or sets the UI for the event raised when a user report is submitting.
	/// </summary>
	[Tooltip("The event raised when a user report is submitting.")]
	public UnityEvent UserReportSubmitting;

	#endregion

	#region Properties

	/// <summary>
	///     Gets the current user report.
	/// </summary>
	public UserReport CurrentUserReport { get; private set; }

	/// <summary>
	///     Gets the current state.
	/// </summary>
	public UserReportingState State
	{
		get
		{
			if (CurrentUserReport != null)
			{
				if (IsInSilentMode)
					return UserReportingState.Idle;
				if (isSubmitting)
					return UserReportingState.SubmittingForm;
				return UserReportingState.ShowingForm;
			}

			if (isCreatingUserReport)
				return UserReportingState.CreatingUserReport;
			return UserReportingState.Idle;
		}
	}

	#endregion

	#region Methods

	/// <summary>
	///     Cancels the user report.
	/// </summary>
	public void CancelUserReport()
	{
		CurrentUserReport = null;
		ClearForm();
	}

	private IEnumerator ClearError()
	{
		yield return new WaitForSeconds(10);
		isShowingError = false;
	}

	private void ClearForm()
	{
		SummaryInput.text = null;
		DescriptionInput.text = null;
	}

	/// <summary>
	///     Creates a user report.
	/// </summary>
	public void CreateUserReport()
	{
		// Check Creating Flag
		if (isCreatingUserReport)
			return;

		// Set Creating Flag
		isCreatingUserReport = true;

		var client = UnityUserReporting.CurrentClient;
		client.ClearScreenshots();

		// Take Main Screenshot
		client.TakeScreenshot(Screen.width / 2, Screen.height / 2);

		// Create Report
		client.CreateUserReport(userReport =>
		{
			// Ensure Project Identifier
			if (string.IsNullOrEmpty(userReport.ProjectIdentifier))
				RRLogger.Error(
					"The user report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityUserReporting.Configure().");

			CreateAttachment(userReport, UserReportEventLevel.Error.ToString(), UserReportEventLevel.Error);
			CreateAttachment(userReport, UserReportEventLevel.Warning.ToString(), UserReportEventLevel.Warning);
			CreateUserAttachment(userReport);
			CreateUserValidateAttachment(userReport);

			// Dimensions
			var platform = "Unknown";
			var version = "0.0";
			foreach (var deviceMetadata in userReport.DeviceMetadata)
			{
				if (deviceMetadata.Name == "Platform")
					platform = deviceMetadata.Value;

				if (deviceMetadata.Name == "Version")
					version = deviceMetadata.Value;
			}

			userReport.Dimensions.Add(new UserReportNamedValue("Platform.Version", $"{platform}.{version}"));

			// Set Current Report
			CurrentUserReport = userReport;

			// Set Creating Flag
			isCreatingUserReport = false;

			// Set Thumbnail
			SetThumbnail(userReport);

			// Submit Immediately in Silent Mode
			if (IsInSilentMode)
				SubmitUserReport();
		});
	}

	private void CreateAttachment(UserReport userReport, string attachmentName, UserReportEventLevel level)
	{
		var stringBuilder = new StringBuilder();
		var events = userReport.Events
			.Where(x => x.Level == level)
			.ToArray();

		var lastIndex = events.Length - 1;
		for (var i = 0; i < 100 && lastIndex >= 0; i++)
		{
			stringBuilder.AppendLine(events[lastIndex].FullMessage);
			lastIndex--;
		}

		var fileName = $"{attachmentName}.json";
		userReport.Attachments.Add(new UserReportAttachment(fileName, fileName, "application/json",
			Encoding.UTF8.GetBytes(stringBuilder.ToString())));
	}

	private void CreateUserAttachment(UserReport userReport)
	{
		if (!User.IsAuthorized)
			return;

		var fileName = $"UserAccountInfo.json";
		userReport.Attachments.Add(new UserReportAttachment(fileName, fileName, "application/json",
			Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(User.Data, Formatting.Indented))));
	}

	private void CreateUserValidateAttachment(UserReport userReport)
	{
		if (!User.IsAuthorized)
			return;
		
		var fileName = $"UserAccountReport.json";
		userReport.Attachments.Add(new UserReportAttachment(fileName, fileName, "application/json",
			Encoding.UTF8.GetBytes(userInventory.ValidateUserData(false))));
	}

	private UserReportingClientConfiguration GetConfiguration()
	{
		return new UserReportingClientConfiguration();
	}

	/// <summary>
	///     Gets a value indicating whether the user report is submitting.
	/// </summary>
	/// <returns>A value indicating whether the user report is submitting.</returns>
	public bool IsSubmitting()
	{
		return isSubmitting;
	}

	private void SetThumbnail(UserReport userReport)
	{
		if (userReport != null && ThumbnailViewer != null)
		{
			var data = Convert.FromBase64String(userReport.Thumbnail.DataBase64);
			var texture = new Texture2D(1, 1);
			texture.LoadImage(data);
			ThumbnailViewer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
				new Vector2(0.5F, 0.5F));
			ThumbnailViewer.preserveAspect = true;
		}
	}

	private void Start()
	{
		// Set Up Event System
		if (Application.isPlaying)
		{
			var sceneEventSystem = FindObjectOfType<EventSystem>();
			if (sceneEventSystem == null)
			{
				var eventSystem = new GameObject("EventSystem");
				eventSystem.AddComponent<EventSystem>();
				eventSystem.AddComponent<StandaloneInputModule>();
			}
		}

		// Configure Client
		var configured = false;
		if (UserReportingPlatform == UserReportingPlatformType.Async)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var asyncUnityUserReportingPlatformType =
				assembly.GetType("Unity.Cloud.UserReporting.Plugin.Version2018_3.AsyncUnityUserReportingPlatform");
			if (asyncUnityUserReportingPlatformType != null)
			{
				var activatedObject = Activator.CreateInstance(asyncUnityUserReportingPlatformType);
				if (activatedObject is IUserReportingPlatform asyncUnityUserReportingPlatform)
				{
					UnityUserReporting.Configure(asyncUnityUserReportingPlatform, GetConfiguration());
					configured = true;
				}
			}
		}

		if (!configured) UnityUserReporting.Configure(GetConfiguration());

		// Ping
		var url =
			$"https://userreporting.cloud.unity3d.com/api/userreporting/projects/{UnityUserReporting.CurrentClient.ProjectIdentifier}/ping";
		UnityUserReporting.CurrentClient.Platform.Post(url, "application/json", Encoding.UTF8.GetBytes("\"Ping\""),
			(upload, download) => {}, (result, bytes) => {});
	}

	/// <summary>
	///     Submits the user report.
	/// </summary>
	public void SubmitUserReport()
	{
		SubmitUserReportAsync().Forget();
	}
	
	public async UniTask SubmitUserReportAsync()
	{
		// Preconditions
		if (isSubmitting || CurrentUserReport == null)
			return;

		// Set Submitting Flag
		isSubmitting = true;

		CurrentUserReport.Summary = $"[{EnvironmentSwitcher.CurrentEnvironment}][{User.UserName ?? "Unauthorized"}]";
		// Set Summary
		if (SummaryInput)
			CurrentUserReport.Summary += $" {SummaryInput.text}";

		// Set Category
		if (CategoryDropdown)
		{
			var optionData = CategoryDropdown.options[CategoryDropdown.value];
			var category = optionData.text;
			CurrentUserReport.Dimensions.Add(new UserReportNamedValue("Category", category));
			CurrentUserReport.Fields.Add(new UserReportNamedValue("Category", category));
		}

		// Set Description
		// This is how you would add additional fields.
		if (DescriptionInput)
		{
			var userReportField = new UserReportNamedValue
			{
				Name = "Description",
				Value = DescriptionInput.text
			};
			CurrentUserReport.Fields.Add(userReportField);
		}

		// Clear Form
		ClearForm();

		// Raise Event
		RaiseUserReportSubmitting();
		var tcs = new UniTaskCompletionSource();
		// Send Report
		UnityUserReporting.CurrentClient.SendUserReport(CurrentUserReport, (uploadProgress, downloadProgress) =>
		{
			if (ProgressText)
				return;

			var progressText = $"{uploadProgress:P}";
			ProgressText.text = progressText;
		}, (success, br2) =>
		{
			if (!success)
			{
				isShowingError = true;
				StartCoroutine(ClearError());
			}

			CurrentUserReport = null;
			isSubmitting = false;
			tcs.TrySetResult();
		});
		await tcs.Task;
	}

	private void Update()
	{
		// Hotkey Support
		if (IsHotkeyEnabled)
			if (Input.GetKey(KeyCode.LeftShift)
			    && Input.GetKey(KeyCode.LeftAlt))
				if (Input.GetKeyDown(KeyCode.B))
					CreateUserReport();

		// Update Client
		UnityUserReporting.CurrentClient.IsSelfReporting = IsSelfReporting;
		UnityUserReporting.CurrentClient.SendEventsToAnalytics = SendEventsToAnalytics;

		// Update UI
		if (UserReportButton)
			UserReportButton.interactable = State == UserReportingState.Idle;

		if (UserReportForm)
			UserReportForm.SetActive(State == UserReportingState.ShowingForm);

		if (SubmittingPopup)
			SubmittingPopup.SetActive(State == UserReportingState.SubmittingForm);

		if (ErrorPopup)
			ErrorPopup.SetActive(isShowingError);

		// Update Client
		// The UnityUserReportingUpdater updates the client at multiple points during the current frame.
		unityUserReportingUpdater.Reset();
		StartCoroutine(unityUserReportingUpdater);
	}

	#endregion

	#region Virtual Methods

	/// <summary>
	///     Occurs when a user report is submitting.
	/// </summary>
	protected virtual void RaiseUserReportSubmitting()
	{
		UserReportSubmitting?.Invoke();
	}

	#endregion
}