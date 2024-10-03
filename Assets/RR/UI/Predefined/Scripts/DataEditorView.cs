using DG.Tweening;
using Newtonsoft.Json.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RR.UI.Predefined
{
	public partial class DataEditorView : BaseView
	{
		public class UploadParams
		{
			public string Name;
			public string Path;
			public string Mime;
			public string UploadUri;
			public string DataType;
			public string Category;
		}

		public class Options
		{
			public class VariableOption
			{
				public IEnumerable<(string name, string[] ext)> FileFilters;
				public bool ShowAllFilesFilter;
				public string DefaultFileFilter;
				public string UploadUri;
				public string UploadMime;
				public string DataType;
				public string Category;

				public Func<JObject, string> ParseResultToUrlString;
			}

			public Dictionary<string, VariableOption> VariableOptions = new Dictionary<string, VariableOption>
			{
				{ "default", new VariableOption()}
			};

			/// <summary>
			/// Set this to true if you want to upload files to server and use special input for each field with *Url in the name.
			/// </summary>
			public bool UrlNameAsFile = false;
			public bool AllowDeleteBtn = true;
			public bool AllowResetBtn = true;
			public bool AllowCancelBtn = true;
			public bool ShowNullableTypes = false;

			public float OpDelay = 20;

			public VariableOption GetFieldOption(string fieldName)
				=> VariableOptions.TryGetValue(fieldName, out var result) ? result : VariableOptions["default"];
		}

		private float hasActiveOperations = -1;
		private Options options = new Options();

		private Dictionary<string, GameObject> spawnedInputs = new Dictionary<string, GameObject>();

		public DataEditorView SetOptions(Options opts)
		{
			options = opts;

			return this;
		}

		protected override void OnShown()
		{
			base.OnShown();

			DeleteBtn.gameObject.SetActive(options.AllowDeleteBtn);
			ResetBtn.gameObject.SetActive(options.AllowResetBtn);
			btCancel.gameObject.SetActive(options.AllowCancelBtn);
		}

		protected override void OnInit()
		{
			base.OnInit();

			MidPannel.ForeachChildren(x => x.gameObject.SetActive(false));
		}

		private IEnumerator ShowLoadDialogCoroutine(Action<string[]> onResult, string fieldName)
		{
			var fieldOption = options.GetFieldOption(fieldName);
			if (fieldOption.FileFilters.Any())
				FileBrowser.SetFilters(
					fieldOption.ShowAllFilesFilter || fieldOption.FileFilters.Any(x => x.ext.Contains("*")),
					fieldOption.FileFilters.Where(x => !x.ext.Contains("*")).Select(x => new FileBrowser.Filter(x.name, x.ext))
				);

			if (string.IsNullOrEmpty(fieldOption.DefaultFileFilter))
				FileBrowser.SetDefaultFilter(fieldOption.DefaultFileFilter);

			yield return FileBrowser.WaitForLoadDialog(false, false, null);

			if (!FileBrowser.Success)
				RRLogger.Log(FileBrowser.Success);

			onResult.Invoke(FileBrowser.Result);
		}

		private IEnumerator UploadFilesRoutine(Action<JObject> onUploaded, params UploadParams[] uploadParams)
		{
			if (uploadParams?.Any() != true)
			{
				RRLogger.Error("There is nothing to upload provided.");
				yield break;
			}

			var files = new UnityWebRequest[uploadParams.Length];
			for (var i = 0; i < files.Length; i++)
			{
				var form = new WWWForm();

				files[i] = UnityWebRequest.Get(uploadParams[i].Path);
				yield return files[i].SendWebRequest();

				if (string.IsNullOrEmpty(uploadParams[i].Mime))
					form.AddBinaryData($"files", files[i].downloadHandler.data, uploadParams[i].Name);
				else form.AddBinaryData($"files", files[i].downloadHandler.data, uploadParams[i].Name, uploadParams[i].Mime);

				if (!string.IsNullOrEmpty(uploadParams[i].DataType))
					form.AddField(nameof(UploadParams.DataType), uploadParams[i].DataType);

				var req = UnityWebRequest.Post(uploadParams[0].UploadUri, form);
				req.timeout = 360;
				yield return req.SendWebRequest();

				if (req.result == UnityWebRequest.Result.ConnectionError 
				    || req.result == UnityWebRequest.Result.ProtocolError)
				{
					RRLogger.Error(req.error);
					continue;
				}

				TryParseResponseJson(req.downloadHandler.text, out var json);
				onUploaded.Invoke(json);
			}
		}

		private bool TryParseResponseJson(string text, out JObject json)
		{
			try
			{
				json = JObject.Parse(text);
				return true;
			}
			catch (Exception ex)
			{
				RRLogger.Error(ex);
			}

			json = new JObject { { "body", new JValue(text) } };
			return false;
		}

		private IEnumerator WaitTillAllOperationsComplete(Action doAfterAllComplete)
		{
			btOk.interactable = false;
			okText.SetText("");
			okText.DOText(".....", 0.76f).SetLoops(-1, LoopType.Restart);

			while (hasActiveOperations > 0)
			{
				hasActiveOperations -= Time.deltaTime;
				yield return null;
			}

			okText.DOKill(true);

			btOk.interactable = true;
			hasActiveOperations = -1;
			doAfterAllComplete();
		}

		private void Update()
		{
			if (!Input.GetKeyDown(KeyCode.Tab))
				return;

			var next = EventSystem.current.currentSelectedGameObject?
				.GetComponent<Selectable>()?
				.FindSelectableOnDown();

			if (next == null)
				return;

			EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
		}

		protected override void Start()
		{
			base.Start();

			FileBrowser.AddQuickLink("Downloads", "D:\\Downloads");
			FileBrowser.AddQuickLink("Downloads", "C:\\Downloads");
			FileBrowser.AddQuickLink(Application.productName, Application.dataPath);

			Subscribe(btCancel, () => Close());
			Subscribe(ResetBtn, () =>
			{
				var supportedFields = new[]
				{
					typeof(int), typeof(decimal), typeof(float), typeof(string),
					typeof(Enum), typeof(List<string>), typeof(bool)
				};
				Data.GetType().GetFields()
					.Where(x => supportedFields.Contains(x.FieldType))
					.ForEach(x => x.SetValue(Data, default));
				ReloadFields();
			});

			Subscribe(DeleteBtn, () =>
			{
				OnDelete?.Invoke();
				Close();
			});

			Subscribe(btOk, SaveData);
		}

		private void SaveData()
		{
			var dataType = Data.GetType();
			dataType.GetFields().ForEach(x =>
			{
				if (x.FieldType.IsEnum)
				{
					var res = spawnedInputs[x.Name].GetComponentInChildren<TMP_Dropdown>();
					var val = res.options[res.value].text;
					var parsed = Enum.Parse(x.FieldType, val);
					x.SetValue(Data, parsed);
					return;
				}

				if (x.FieldType == typeof(string))
				{
					if (options.UrlNameAsFile && x.Name.ToLower().Contains("url"))
					{
						var fileRes = spawnedInputs[x.Name]
							.GetComponentInChildren<Button>()
							.GetComponentInChildren<Text>().text;

						if (string.IsNullOrEmpty(fileRes))
						{
							var rs = spawnedInputs[x.Name].GetComponentInChildren<TMP_InputField>().text;
							x.SetValue(Data, rs);
							return;
						}

						try
						{
							var nameValue = dataType.GetField("Name", BindingFlags.IgnoreCase) ??
											dataType.GetField("Title", BindingFlags.IgnoreCase);
							var opts = options.GetFieldOption(x.Name);
							hasActiveOperations = options.OpDelay;
							StartCoroutine(
								UploadFilesRoutine(s =>
								{
									var result = opts.ParseResultToUrlString?.Invoke(s) ?? s.ToString();
									x.SetValue(Data, result);
									hasActiveOperations = -1;
								}, new UploadParams
								{
									Mime = opts.UploadMime,
									Name = nameValue?.GetValue(x).ToString() ?? x.Name,
									Path = fileRes,
									UploadUri = opts.UploadUri,
									DataType = opts.DataType,
									Category = opts.Category
								})
							);
						}
						catch (Exception ex)
						{
							RRLogger.Error(ex);
							hasActiveOperations = -1;
						}

						return;
					}

					var res = spawnedInputs[x.Name].GetComponentInChildren<TMP_InputField>().text;
					x.SetValue(Data, res);
					return;
				}

				if (x.FieldType == typeof(int) || x.FieldType == typeof(float) || x.FieldType == typeof(decimal)
					|| options.ShowNullableTypes &&
					(x.FieldType == typeof(int?) || x.FieldType == typeof(float?) ||
					 x.FieldType == typeof(decimal?)))
				{
					var res = spawnedInputs[x.Name].GetComponentInChildren<TMP_InputField>().text;

					if (x.FieldType == typeof(int))
					{
						x.SetValue(Data, int.Parse(res));
						return;
					}

					if (x.FieldType == typeof(float))
					{
						x.SetValue(Data, float.Parse(res));
						return;
					}

					if (x.FieldType == typeof(decimal))
					{
						x.SetValue(Data, decimal.Parse(res));
						return;
					}

					return;
				}

				if (x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(List<>))
				{
					if (x.FieldType != typeof(List<string>))
						return;

					var res = spawnedInputs[x.Name].GetComponentInChildren<TMP_InputField>().text;
					var r = res.Split('\r', '\n', ',').Select(t => t.Trim()).ToList();
					x.SetValue(Data, r);
					return;
				}

				if (x.FieldType == typeof(bool) || options.ShowNullableTypes && (x.FieldType == typeof(bool?)))
				{
					var res = spawnedInputs[x.Name].GetComponentInChildren<Toggle>().isOn;
					x.SetValue(Data, res);
					return;
				}
			});

			StartCoroutine(WaitTillAllOperationsComplete(() =>
			{
				OnSaved?.Invoke(Data);
				Close();
			}));
		}

		protected override void OnBuild(bool isFirstBuild)
		{
			ReloadFields();
		}

		private void ReloadFields()
		{
			if (Data == null)
			{
				RRLogger.Error("Data cannot be null, please pass reference");
				return;
			}

			if (options == null)
			{
				options = new Options();
				RRLogger.Log($"Data editor for [{Data.GetType().Name.DarkGreen()}] Using default options...");
			}

			spawnedInputs.Values.ForEach(x => Destroy(x));
			spawnedInputs.Clear();

			okText.SetText("Ok");

			var dataType = Data.GetType();
			Title.SetText($"Editor [{dataType.Name.Green()}]");

			dataType.GetFields().ForEach(x =>
			{
				if (x.FieldType.IsEnum)
				{
					// todo: multi select on flags
					//if (x.FieldType.GetCustomAttributes(typeof(FlagsAttribute),false).Any())

					InputDropdownFieldName.SetText(x.Name);
					var values = x.FieldType.GetEnumNames().ToList();
					InputDropdownDropdown.ClearOptions();
					InputDropdownDropdown.AddOptions(values);

					var value = x.GetValue(Data)?.ToString();
					var option = InputDropdownDropdown.options.FirstOrDefault(d => d.text == value);
					InputDropdownDropdown.value = InputDropdownDropdown.options.IndexOf(option);

					var sInp = Instantiate(InputDropdown.gameObject, MidPannel);
					spawnedInputs.Add(x.Name, sInp);
				}

				if (x.FieldType == typeof(string))
				{
					if (options.UrlNameAsFile && x.Name.ToLower().Contains("url"))
					{
						InputFileFieldName.SetText(x.Name);
						InputFileInputField.text = x.GetValue(Data)?.ToString();

						var fileInp = Instantiate(InputFile.gameObject, MidPannel);
						var btn = fileInp.GetComponentInChildren<Button>();
						btn.onClick.AddListener(() =>
							StartCoroutine(
								ShowLoadDialogCoroutine(r =>
								{
									if (r == null)
									{
										btn.GetComponentInChildren<Text>(true).text = string.Empty;
										btn.GetComponentInChildren<TMP_Text>(true).text = string.Empty;
										return;
									}

									btn.GetComponentInChildren<Text>(true).text = string.Join(",", r);
									btn.GetComponentInChildren<TMP_Text>(true).text = string.Join(",", r);
								}, x.Name)
							)
						);

						spawnedInputs.Add(x.Name, fileInp);
						return;
					}

					InputOneLineFieldName.SetText(x.Name);
					InputOneLineInputField.text = x.GetValue(Data)?.ToString();
					var sInp = Instantiate(InputOneLine.gameObject, MidPannel);
					spawnedInputs.Add(x.Name, sInp);
					return;
				}

				if (x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(List<>))
				{
					if (x.FieldType != typeof(List<string>))
						return;

					var data = x.GetValue(Data);
					InputMultyLineInputField.text = data == null ? string.Empty : string.Join("\n", (List<string>)data);
					InputMultyLineFieldName.SetText(x.Name);
					var sInp = Instantiate(InputMultyLine.gameObject, MidPannel);
					spawnedInputs.Add(x.Name, sInp);
					return;
				}

				if (x.FieldType == typeof(int) || x.FieldType == typeof(float) || x.FieldType == typeof(decimal)
					|| options.ShowNullableTypes && (x.FieldType == typeof(int?) || x.FieldType == typeof(float?) || x.FieldType == typeof(decimal?)))
				{
					InputDecimalFieldName.SetText(x.Name);
					InputDecimalInputField.text = x.GetValue(Data)?.ToString();
					InputDecimalInputField.contentType = x.FieldType == typeof(int)
						? TMP_InputField.ContentType.IntegerNumber
						: TMP_InputField.ContentType.DecimalNumber;
					var sInp = Instantiate(InputDecimal.gameObject, MidPannel);
					spawnedInputs.Add(x.Name, sInp);
					return;
				}

				if (x.FieldType == typeof(bool) || options.ShowNullableTypes && (x.FieldType == typeof(bool?)))
				{
					InputBoolFieldName.SetText(x.Name);
					InputBoolToggle.isOn = x.GetValue(Data)?.Equals(true) == true;
					var sInp = Instantiate(InputBool.gameObject, MidPannel);
					spawnedInputs.Add(x.Name, sInp);
					return;
				}
			});

			spawnedInputs.ForEach(x => x.Value.SetActive(true));
			EventSystem.current.SetSelectedGameObject(spawnedInputs.Values.FirstOrDefault());
		}

		protected override void OnChanged()
		{
			//copy data from UI controls to data object
		}
	}
}