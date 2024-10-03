using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RR.Core.Components;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RR.UI.DebugSystem
{
	public class RRConsole : Singleton<RRConsole>
	{
		// todo: move to config
		public bool forceShow;
		public int maxLines = 40;

		public delegate string Command(string[] args);

		private bool IsShowing => content.activeInHierarchy;

		[SerializeField] private TMP_Text consoleText = null;
		[SerializeField] private Button submitButton = null;
		[SerializeField] private TMP_InputField inputField = null;

		[SerializeField] private GameObject background = default;
		[SerializeField] private GameObject content = default;
		[SerializeField] private CanvasGroup canvasGroup = default;

		private LinkedList<string> commandsList = new();
		private LinkedListNode<string> actualCommand;

		private static readonly Dictionary<string, CommandInfo> defaultCmdMap = new()
		{
			["help"] = new CommandInfo(Help, "Shows available commands"),
			["?"] = new CommandInfo(Help, "Shows available commands"),
			["exit"] = new CommandInfo(Exit, "Close console (same as Esc)"),
			["cls"] = new CommandInfo(Clear, "Clears console"),
			["qqq"] = new CommandInfo(QuitGame, "Exits game without confirmation"),
			["opacity"] = new CommandInfo(ConsoleOpacity, "Set console opacity"),
			["color"] = new CommandInfo(ConsoleColor, "Set console text color")
		};

		private static readonly Dictionary<string, CommandInfo> cmdMap = new();

		private void Start()
		{
			submitButton.onClick.AddListener(Submit);

			// Force activation of console during splash screen
			if (forceShow)
				Show();
			else
				Hide();

			Clear();
			cmdMap.AddRange(defaultCmdMap);
		}

		private void Update()
		{
			if (IsShowing && Input.GetKeyDown(KeyCode.Return))
				Submit();

			if (Input.GetKeyDown(KeyCode.BackQuote) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
			{
				Show(!IsShowing);
			}

			// control prev/next commands in console
			if (IsShowing && commandsList.Any())
			{
				if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					inputField.text = (actualCommand = actualCommand?.Previous ?? commandsList.Last).Value;
					inputField.MoveTextEnd(false);
				}

				if (Input.GetKeyDown(KeyCode.DownArrow))
				{
					inputField.text = (actualCommand = actualCommand?.Next ?? commandsList.First).Value;
					inputField.MoveTextEnd(false);
				}
			}
		}

		private void Submit()
		{
			if (string.IsNullOrEmpty(inputField.text))
			{
				PrintToConsole("");
				return;
			}

			PrintToConsole(inputField.text);
			PrintToConsole(ParseCommand(inputField.text));
			inputField.text = string.Empty;
			inputField.ActivateInputField();
		}

		public static void AddCommand(string key, Command action, string helpText = "")
		{
			if (string.IsNullOrEmpty(key))
			{
				RRLogger.Error("Command key cant be null.");
				return;
			}

			if (key.Split(',', ' ').Length > 1)
			{
				RRLogger.Error("Key must be one single word without commas and spaces!");
				return;
			}

			key = key.ToLower();
			if (cmdMap.ContainsKey(key))
			{
				cmdMap[key] = new CommandInfo(action, helpText);
				return;
			}

			cmdMap.Add(key, new CommandInfo(action, helpText));
		}

		public static void RemoveCommands()
		{
			cmdMap.Clear();
			cmdMap.AddRange(defaultCmdMap);
		}

		public static void PrintToConsole(string input)
		{
			if (string.IsNullOrEmpty(input))
				return;

			var nextLines = new[] { "\r\n", "\r" };

			// Split into list
			var lines = new List<string>();
			lines.AddRange(Instance.consoleText.text.Split(nextLines, StringSplitOptions.None));

			var inputLines = input.Split(nextLines, StringSplitOptions.None).ToList();
			for (var i = 0; i < inputLines.Count; i++)
				inputLines[i] = $"[{DateTime.Now.ToLongTimeString()}]: {inputLines[i]}";

			lines.AddRange(inputLines);

			// Truncate list (dropping oldest line)
			if (lines.Count > Instance.maxLines)
				lines = lines.GetRange(lines.Count - Instance.maxLines, Instance.maxLines);

			// Add new line
			Instance.consoleText.text = string.Join("\n", lines);
		}

		public void Show(bool value = true)
		{
			content.SetActive(value);
			background.SetActive(value);

			if (value)
				inputField.ActivateInputField();
			else
				inputField.DeactivateInputField();
		}

		public void Hide() => Show(false);

		private string ParseCommand(string cmd)
		{
			// Remember last  command
			commandsList.AddLast(new LinkedListNode<string>(cmd));

			var args = cmd.Trim().Split(' ', ',').ToList();
			if (!cmdMap.TryGetValue(args[0].ToLower(), out var callback))
				return $"{cmd}: command not found";

			args.RemoveAt(0); // remove first arg
			var result = callback.Cmd.Invoke(args.ToArray());

			RRLogger.Log(cmd);

			return result;
		}

		#region Command Methods

		private static string Help(params string[] args)
		{
			var sb = new StringBuilder("Command List:\n");
			foreach (var c in cmdMap)
			{
				sb.Append("\t\t");
				sb.Append(c.Key.Bold());
				sb.Append("\t\t");
				sb.Append(c.Value.Help);
				sb.Append("\n");
			}
			return sb.ToString();
		}

		private static string Clear(params string[] args)
		{
			Instance.consoleText.text = string.Empty;
			return null;
		}

		private static string Exit(params string[] args)
		{
			Instance.Hide();
			return null;
		}

		private static string QuitGame(params string[] args)
		{
			Application.Quit();
			return "Closing game client";
		}

		private static string ConsoleColor(params string[] args)
		{
			if (args.Length == 0)
			{
				Instance.consoleText.color = Color.white;
				return "White";
			}

			var unityColors = typeof(Color)
				.GetProperties()
				.Where(x => x.IsStatic() && x.PropertyType == typeof(Color));

			var color = (Color)(
				unityColors.FirstOrDefault(x => x.Name == args[0])?.GetValue(null)
				?? args[0].HexToColor()
			);

			Instance.consoleText.color = color;

			return $"Console color set to {color.ColorToHex()} ({args[0]})";
		}

		private static string ConsoleOpacity(params string[] args)
		{
			if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
				return Instance.canvasGroup.alpha.ToString("0.00");

			if (!float.TryParse(args[0].Replace('.', ','), out var opacity))
			{
				if (args[0] != "clear")
					return $"first argument is not a float '{args[0]}'";

				Instance.canvasGroup.alpha = 0.75f; // default value
				return null;
			}

			Instance.canvasGroup.alpha = opacity;
			return $"Console opacity set to {opacity:0.00}";
		}

		#endregion

		private struct CommandInfo
		{
			public readonly Command Cmd;
			public readonly string Help;

			public CommandInfo(Command cmd, string help)
			{
				Cmd = cmd;
				Help = help;
			}
		}
	}
}
