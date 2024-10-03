using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BerserkV3.Common.SceneService;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Utilities;
using RR.UI.DebugSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Log
{
	public sealed class LogView : MonoBehaviour
	{
		[Flags]
		private enum Mode : short
		{
			None = 0,
			Info = 1,
			Warning = 2,
			Error = 4,
			All = 7
		}

		private struct LogEntry
		{
			public readonly Mode LogType;
			public readonly long TimeStamp;
			public readonly string Message;

			public LogEntry(Mode logType, long timestamp, string message)
			{
				LogType = logType;
				TimeStamp = timestamp;
				Message = message;
			}
		}

		private static readonly int MAX_LOG_CAPACITY = 1000;
		private static readonly int DISPLAY_LOG_CAPACITY = 100;

		[SerializeField] private GameObject container;
		[SerializeField] private TextMeshProUGUI logsText;

		[SerializeField] private Button closeButton;
		[SerializeField] private Button openButton;
		[SerializeField] private Button reconnectButton;
		[SerializeField] private Button cacheButton;
		[SerializeField] private Button consoleButton;
		[SerializeField] private Button deviceIdButton;
		[SerializeField] private Button enterShowRoomButton;
		[SerializeField] private Button exitShowRoomIdButton;

		[SerializeField] private Toggle infoButton;
		[SerializeField] private Toggle warningButton;
		[SerializeField] private Toggle errorButton;

		[SerializeField] private Button copyButton;
		[SerializeField] private Button clearButton;
		[SerializeField] private RRConsole console;

		//TODO: Replace with pre-initialized array cyclical buffer
		private readonly Queue<LogEntry> log = new Queue<LogEntry>(MAX_LOG_CAPACITY);

		private Mode currentMode = Mode.All;
		private bool requiresUpdate;

		private readonly Regex tagRegEx = new Regex("<([^>])*>", RegexOptions.Compiled);

		private void Awake()
		{
			reconnectButton.gameObject.SetActive(false);
			SceneServiceAdapter.Service.OnSceneLoaded += OnSceneLoaded;
			Application.logMessageReceived += HandleLog;

			closeButton.Subscribe(() => Toggle(false));
			openButton.Subscribe(() => Toggle(true));
			reconnectButton.Subscribe(() => GameCore.GameCoreBus.OnReconnectRequired += true);
			deviceIdButton.Subscribe(() => SystemInfo.deviceUniqueIdentifier.CopyToClipboard());
			consoleButton.Subscribe(() => console.Show());
			cacheButton.Subscribe(RRFile.OpenSaveFolder);

			warningButton.onValueChanged.AddListener(v => SelectMode(Mode.Warning, v));
			errorButton.onValueChanged.AddListener(v => SelectMode(Mode.Error, v));
			infoButton.onValueChanged.AddListener(v => SelectMode(Mode.Info, v));

			exitShowRoomIdButton.onClick.AddListener(LoadStartup);
			enterShowRoomButton.onClick.AddListener(LoadShowRoom);
			
			copyButton.onClick.AddListener(OnCopy);
			clearButton.onClick.AddListener(OnClear);
			RRLogger.Log($"{nameof(LogView)}.Awake end.");
		}

		private void OnSceneLoaded(Scene scene)
		{
			if (reconnectButton)
				reconnectButton.gameObject.SetActive(scene == Scene.Game);
						
			if (exitShowRoomIdButton)
				exitShowRoomIdButton.gameObject.SetActive(scene == Scene.ShowRoom);
			
			if (enterShowRoomButton)
				enterShowRoomButton.gameObject.SetActive(scene == Scene.Lobby);
		}
		
		private void LoadStartup()
		{
			SceneServiceAdapter.Service.LoadAsync(Scene.StartUp);
		}

		private void LoadShowRoom()
		{
			SceneServiceAdapter.Service.LoadAsync(Scene.ShowRoom);
		}
		
		private void Toggle(bool show)
		{
			container.SetActive(show);

			if (show)
				Render();
		}

		private void Render()
		{
			if (container == null)
				return;

			if (!container.activeSelf)
				return;

			var renderedText = "";

			log.Where(l => currentMode.HasFlag(l.LogType))
				.OrderBy(x => x.TimeStamp)
				.Skip(log.Count - DISPLAY_LOG_CAPACITY)
				.ForEach(t => renderedText = $"{renderedText}\n[{t.LogType}] : {t.Message}\n");

			logsText.SetText(renderedText ?? string.Empty);
		}

		private void LateUpdate()
		{
			if (requiresUpdate)
			{
				requiresUpdate = false;
				Render();
			}
		}

		private void OnCopy()
		{
			var fullLog = "";

			log.Where(l => currentMode.HasFlag(l.LogType))
				.OrderBy(x => x.TimeStamp)
				.ForEach(t => fullLog = $"{fullLog}\n{FormatLogEntry(t)}");

			var dateTime = DateTime.Now.ToBinary();
			var logFileName = $"{dateTime}_[{currentMode}].log";

			SaveLog();

			void SaveLog()
			{
				WebGLSaveTextFile.Save(fullLog, logFileName);
				RRFile.Save(logFileName, fullLog);
			}
		}

		private void OnClear()
		{
			log.Clear();
			Render();
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (ShouldIgnoreLog(logString, stackTrace))
				return;

			var mode = Mode.Info;
			if (type == LogType.Error) mode = Mode.Error;
			if (type == LogType.Warning) mode = Mode.Warning;

			if (log.Count >= MAX_LOG_CAPACITY)
				log.Dequeue();

			log.Enqueue(new LogEntry(mode, DateTime.Now.ToFileTime(), SanitizeLogLine(logString)));
			requiresUpdate = true;
		}

		private void SelectMode(Mode mode, bool on)
		{
			currentMode = currentMode ^ mode;
			Render();
		}

		private string SanitizeLogLine(string log)
		{
			if (!logsText.font.HasCharacters(log, out var missingCaracters))
				missingCaracters.ForEach(character => log = log.Replace(character, default));

			return log;
		}

		private bool ShouldIgnoreLog(string log, string stack)
		{
			if (stack.Contains("LogView"))
				return true;

			if (log.Contains("DOTWEEN"))
				return true;

			if (log.Contains(@"\health\"))
				return true;

			return false;
		}

		private string FormatLogEntry(LogEntry logEntry)
		{
			var dateTime = DateTime.FromFileTimeUtc(logEntry.TimeStamp);
			var time = dateTime.ToString("hh:mm:ss");

			var text = tagRegEx.Replace(logEntry.Message, string.Empty);

			return $"[{logEntry.LogType} at {time}] {text} \n";
		}

		private void OnDestroy()
		{
			SceneServiceAdapter.Service.OnSceneLoaded -= OnSceneLoaded;
		}
	}
}