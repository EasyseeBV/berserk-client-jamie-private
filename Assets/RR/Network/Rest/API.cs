using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BestHTTP;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace RR.Network.Rest
{
	public struct APIResponse<T> where T : class
	{
		public APIResponse(HTTPResponse response, Func<HTTPResponse, T> customDeserialization = null)
		{
			Code = (HttpStatusCode)response.StatusCode;
			IsSuccess = response.IsSuccess;
			RawMessage = response.DataAsText;
			StatusCodeMessage = $"{response.StatusCode.ToString().Bold()}:{response.DataAsText}";

			if (!response.IsSuccess)
			{
				ErrorMessage = response.Message;
				Data = default;
				return;
			}

			if (response.DataAsText is T stringData)
			{
				ErrorMessage = string.Empty;
				Data = stringData;
				return;
			}

			try
			{
				Data = customDeserialization != null
					? customDeserialization(response)
					: JsonConvert.DeserializeObject<T>(response.DataAsText);

				ErrorMessage = string.Empty;
			}
			catch (JsonException ex)
			{
				ErrorMessage = ex.Message;
				Data = default;
			}
		}
		
		public string ErrorMessage { get; set; }
		public string RawMessage { get; set; }
		public string StatusCodeMessage { get; set; }
		public T Data { get; set; }
		public HttpStatusCode Code { get; set; }
		public bool IsSuccess { get; set; }

		//public static implicit operator T(APIResponse<T> response) => response.Data;
		public static implicit operator HttpStatusCode(APIResponse<T> response) => response.Code;
		public static implicit operator bool(APIResponse<T> response) => response.IsSuccess;
	}

	public class APIException : Exception
	{
		public APIException(string msg) : base(msg) { }
	}

	/// <summary>
	/// Example:
	///	<br/> public class GameAPI : API&lt;GameAPI&gt;
	///	<br/> {
	///	<br/> 		public override string BaseUrl => "https://ccgvulcanserver.azurewebsites.net/api/v1";
	///	<br/> 		public override string AuthToken => null; // PlayerPrefs.GetString("myToken");
	///	<br/> }
	///	<para/>
	///			GameAPI.Post&lt;TReturn&gt;(...);
	/// </summary>
	public abstract class API<TK> where TK : API<TK>, new()
	{
		protected static event Action<Exception> OnError;
		protected static event Action<int, string> OnRequestFail;
		protected static event Action<string> OnRequest;
		protected static event Action<string> OnResponse;

		public abstract string BaseUrl { get; }
		public virtual string AuthToken { get; } = string.Empty;
		/// <summary>
		/// Body will be hidden if the path contains those words.
		/// </summary>
		public virtual string[] SensitivePathWords { get; } = Enumerable.Empty<string>().ToArray();

		#region Static setup magic

		protected static TK Instance;
		static API()
		{
			Instance = new TK();
			Instance.OnInit();
		}

		/// <summary>
		/// Use this method to subscribe to API events.
		/// <br/>Example:
		/// <br/>		protected override void OnInit()
		/// <br/>		{
		/// <br/>			OnRequestFail += (x, i) => UIManager.ShowWarningDialog($"<color=red><b>Error: {x}</b></color>\n{i}");
		/// <br/>			OnRequest += x => RRLogger.Log(x);
		/// <br/>			OnResponse += x => RRLogger.Log(x);
		/// <br/>		}
		/// </summary>
		protected virtual void OnInit() { }

		#endregion

		#region HTTP Shortcuts

		public static async Task<APIResponse<T>> GetAsync<T>(string route, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Get, null, true, customDeserialization);
		
		public static async Task<APIResponse<T>> PostAsync<T>(string route, string jsonContent, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Post, jsonContent, false, customDeserialization);

		public static async Task<APIResponse<T>> PatchAsync<T>(string route, string jsonContent, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Patch, jsonContent, false, customDeserialization);

		public static async Task<APIResponse<T>> PutAsync<T>(string route, string jsonContent, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Put, jsonContent, false, customDeserialization);

		public static async Task<APIResponse<T>> DeleteAsync<T>(string route, string jsonContent, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Delete, jsonContent, false, customDeserialization);
		
		public static async Task<APIResponse<T>> OptionsAsync<T>(string route, string jsonContent, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Options, jsonContent, false, customDeserialization);
		
		public static void Get<T>(string route, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Get, null, true, onRequestFinishedCallback, customDeserialization);

		public static void Get<T>(string route, string jsonContent = null, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Get, jsonContent, false, onRequestFinishedCallback, customDeserialization);

		public static void Post<T>(string route, string jsonContent, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Post, jsonContent, false, onRequestFinishedCallback, customDeserialization);

		public static void Patch<T>(string route, string jsonContent, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Patch, jsonContent, false, onRequestFinishedCallback, customDeserialization);

		public static void Put<T>(string route, string jsonContent, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Put, jsonContent, false, onRequestFinishedCallback, customDeserialization);

		public static void Delete<T>(string route, string jsonContent, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Delete, jsonContent, false, onRequestFinishedCallback, customDeserialization);
		public static async void Options<T>(string route, string jsonContent, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Options, jsonContent, false, onRequestFinishedCallback, customDeserialization);
		
		public static async Task<APIResponse<T>> PostAsync<T>(string route, object content, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Post, content, false, customDeserialization);

		public static async Task<APIResponse<T>> PatchAsync<T>(string route, object content, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Patch, content, false, customDeserialization);

		public static async Task<APIResponse<T>> PutAsync<T>(string route, object content, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Put, content, false, customDeserialization);

		public static async Task<APIResponse<T>> DeleteAsync<T>(string route, object content, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Delete, content, false, customDeserialization);

		public static async Task<APIResponse<T>> OptionsAsync<T>(string route, object content, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Options, content, false, customDeserialization);
		
		public static async Task<APIResponse<T>> PostAsync<T>(string route, Dictionary<string, string> form, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Post, form, true, customDeserialization);

		public static async Task<APIResponse<T>> PatchAsync<T>(string route, Dictionary<string, string> form, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Patch, form, true, customDeserialization);

		public static async Task<APIResponse<T>> PutAsync<T>(string route, Dictionary<string, string> form, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Put, form, true, customDeserialization);

		public static async Task<APIResponse<T>> DeleteAsync<T>(string route, Dictionary<string, string> form, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Delete, form, true, customDeserialization);
		public static async Task<APIResponse<T>> OptionsAsync<T>(string route, Dictionary<string, string> form, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Options, form, true, customDeserialization);
		public static void Get<T>(string route, object content = null, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Get, content, false, onRequestFinishedCallback, customDeserialization);

		public static void Post<T>(string route, object content, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Post, content, false, onRequestFinishedCallback, customDeserialization);

		public static void Patch<T>(string route, object content, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Patch, content, false, onRequestFinishedCallback, customDeserialization);

		public static void Put<T>(string route, object content, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Put, content, false, onRequestFinishedCallback, customDeserialization);

		public static void Delete<T>(string route, object content, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Delete, content, false, onRequestFinishedCallback, customDeserialization);

		public static void Options<T>(string route, object content, Action<APIResponse<T>> onRequestFinishedCallback = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Options, content, false, onRequestFinishedCallback, customDeserialization);
		
		public static void Get<T>(string route, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Get(route, default, onSuccess, onFailed, customDeserialization);

		public static void Get<T>(string route, object content = null, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Get, content, false, x => RouteResult(onSuccess, onFailed, x), customDeserialization);

		public static void Post<T>(string route, object content, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Post, content, false, x => RouteResult(onSuccess, onFailed, x), customDeserialization);

		public static void Patch<T>(string route, object content, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Patch, content, false, x => RouteResult(onSuccess, onFailed, x), customDeserialization);

		public static void Put<T>(string route, object content, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Put, content, false, x => RouteResult(onSuccess, onFailed, x), customDeserialization);

		public static void Delete<T>(string route, object content, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Delete, content, false, x => RouteResult(onSuccess, onFailed, x), customDeserialization);
		public static void Options<T>(string route, object content, Action<T> onSuccess = null, Action<HttpStatusCode, string> onFailed = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
			=> Instance.FormAndSendRequest(route, HTTPMethods.Options, content, false, x => RouteResult(onSuccess, onFailed, x), customDeserialization);		
		public static void Get(string route, object content = null, Action<APIResponse<string>> onRequestFinishedCallback = null)
			=> Instance.FormAndSendRequest(route, HTTPMethods.Get, content, false, onRequestFinishedCallback);

		public static void Post(string route, object content, Action<APIResponse<string>> onRequestFinishedCallback = null)
			=> Instance.FormAndSendRequest(route, HTTPMethods.Post, content, false, onRequestFinishedCallback);

		public static void Patch(string route, object content, Action<APIResponse<string>> onRequestFinishedCallback = null)
			=> Instance.FormAndSendRequest(route, HTTPMethods.Patch, content, false, onRequestFinishedCallback);

		public static void Put(string route, object content, Action<APIResponse<string>> onRequestFinishedCallback = null)
			=> Instance.FormAndSendRequest(route, HTTPMethods.Put, content, false, onRequestFinishedCallback);

		public static void Delete(string route, object content, Action<APIResponse<string>> onRequestFinishedCallback = null)
			=> Instance.FormAndSendRequest(route, HTTPMethods.Delete, content, false, onRequestFinishedCallback);
		public static void Options(string route, object content, Action<APIResponse<string>> onRequestFinishedCallback = null) => 
			Instance.FormAndSendRequest(route, HTTPMethods.Options, content, false, onRequestFinishedCallback);
		
		public static async Task<HttpStatusCode> GetAsync(string route)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Get, null);

		public static async Task<HttpStatusCode> PostAsync(string route, string jsonContent)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Post, jsonContent);

		public static async Task<HttpStatusCode> PatchAsync(string route, string jsonContent)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Patch, jsonContent);

		public static async Task<HttpStatusCode> PutAsync(string route, string jsonContent)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Put, jsonContent);

		public static async Task<HttpStatusCode> DeleteAsync(string route, string jsonContent)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Delete, jsonContent);

		public static async Task<HttpStatusCode> OptionsAsync(string route, string jsonContent) => 
			await Instance.FormAndSendRequestAsync(route, HTTPMethods.Options,jsonContent);
		
		public static async Task<HttpStatusCode> PostAsync(string route, object content)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Post, content);

		public static async Task<HttpStatusCode> PatchAsync(string route, object content)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Patch, content);

		public static async Task<HttpStatusCode> PutAsync(string route, object content)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Put, content);

		public static async Task<HttpStatusCode> DeleteAsync(string route, object content)
			=> await Instance.FormAndSendRequestAsync(route, HTTPMethods.Delete, content);
		public static async Task<HttpStatusCode> OptionsAsync(string route, object content) => 
			await Instance.FormAndSendRequestAsync(route, HTTPMethods.Options, content);

		#endregion

		public async Task<HttpStatusCode> FormAndSendRequestAsync(string route, HTTPMethods method, object content)
			=> await FormAndSendRequestAsync<string>(route, method, content);

		public void FormAndSendRequest(string route, HTTPMethods method, object content)
			=> FormAndSendRequest<string>(route, method, content);

		public async Task<APIResponse<T>> FormAndSendRequestAsync<T>(string route, HTTPMethods method, object content, bool isForm = false, Func<HTTPResponse, T> customDeserialization = null) where T : class
		{
			if (!IsUrlValid(BaseUrl))
				return default;

			try
			{
				var request = PrepareRequest(route, method, content, isForm, null);
				var response = await request.GetHTTPResponseAsync();

				return FormatResponse(request, response, customDeserialization);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return default;
			}
		}

		public void FormAndSendRequest<T>(string route, HTTPMethods method, object content, bool isForm = false, Action<APIResponse<T>> onRequestFinished = null, Func<HTTPResponse, T> customDeserialization = null) where T : class
		{
			if (!IsUrlValid(route))
				return;

			PrepareRequest(route, method, content, isForm, (request, response) =>
			{
				var result = FormatResponse(request, response, customDeserialization);
				onRequestFinished?.Invoke(result);
			}).Send();
		}

		protected HTTPRequest PrepareRequest(string route, HTTPMethods method, object content, bool isForm, OnRequestFinishedDelegate callback)
		{
			var uri = route.StartsWith("https://") || route.StartsWith("http://")
				? new Uri(route)
				: new Uri($"{BaseUrl}/{route}");

			var request = callback != null
				? new HTTPRequest(uri, method, callback)
				: new HTTPRequest(uri, method);

			if (isForm || method == HTTPMethods.Get) 
				PrepareFormContent(method, content, request);
			else 
				PrepareBodyContent(method, content, request);

			PrepareHeaders(request);

			OnRequest?.Invoke(FormatLogOutput(method, content, request));
			return request;
		}

		protected virtual void PrepareBodyContent(HTTPMethods method, object content, HTTPRequest request)
		{
			if (content == null)
				return;

			var json = SafeSerialize(content);
			request.RawData = Encoding.UTF8.GetBytes(json);
		}

		protected virtual void PrepareFormContent(HTTPMethods method, object content, HTTPRequest request)
		{
			if (content == null)
				return;

			switch (content)
			{
				case string textContent:
					// todo: add text-content to formFields converter
					OnError?.Invoke(new APIException("String content not supported for the FORMS parsing atm. Use model object with properties."));
					return;
				case Dictionary<string, string> formDictionary:
					formDictionary.ForEach(f => request.AddField(f.Key, f.Value));
					return;
				default: // fallback to create form from the provided content
					void PrepareField(PropertyInfo x) => request.AddField(x.Name, x.GetValue(content)?.ToString());
					content
						.GetType()
						.GetProperties()
						.ForEach(PrepareField);
					return;
			}
		}

		protected virtual void PrepareHeaders(HTTPRequest request)
		{
			request.SetHeader("Content-Type", WebConfig.ContentType);
#if TIME_HEADER
			request.SetHeader("req_token", Convert.ToBase64String(BitConverter.GetBytes(DateTime.UtcNow.ToBinary())));
#endif
			if (AuthToken != null) request.SetHeader("Authorization", $"Bearer {AuthToken}");
		}

		private static APIResponse<T> FormatResponse<T>(HTTPRequest request, HTTPResponse response, Func<HTTPResponse, T> customDeserialization = null) where T : class
		{
			if (response == null)
			{
				OnError?.Invoke(new Exception("No internet connection"));
				return default;
			}

			try
			{
				var format = response.IsSuccess
				? (request.MethodType + " Response").Lightblue()
				: (request.MethodType + " Response").Red().Bold();

				var secure = Instance.SensitivePathWords.Any(request.Uri.AbsolutePath.ToLower().Contains);
				var responseLog = $"[{format} {response.StatusCode.ToString().Bold()}] {request.Uri} {response.Message}\n{(secure ? "" : response.DataAsText)}";

				OnResponse?.Invoke(responseLog);
				if (!response.IsSuccess) 
					OnRequestFail?.Invoke(response.StatusCode, response.DataAsText);

				return new APIResponse<T>(response, customDeserialization);
			}
			catch (Exception ex)
			{
				OnResponse?.Invoke(ex.Message);
				OnError?.Invoke(ex);
				return default;
			}
		}

		private string FormatLogOutput(HTTPMethods method, object content, HTTPRequest request)
		{
			var requestUrl = request.CurrentUri.ToString().ToLower();
			var secure = SensitivePathWords.Any(requestUrl.Contains);
			var formattedMsg = $"[{(method + " Request").Pink()}] {requestUrl}\n{(secure ? string.Empty : SafeSerialize(content))}";
			return formattedMsg;
		}

		private bool IsUrlValid(string baseUrl)
		{
			if (Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
				return true;

			var formattedMessage = $"Please provide correct {nameof(BaseUrl)} before sending requests. {GetType().Name}.{nameof(BaseUrl)}";
			OnError?.Invoke(new APIException(formattedMessage));
			return false;
		}

		private static void RouteResult<T>(Action<T> onSuccess, Action<HttpStatusCode, string> onFailed, APIResponse<T> response) where T : class
		{
			if (response)
			{
				onSuccess?.Invoke(response.Data);
				return;
			}

			onFailed?.Invoke(response.Code, response.RawMessage);
		}

		private static string SafeSerialize(object content)
			=> content == null ? string.Empty
				: content is string textContent ? textContent
					: JsonConvert.SerializeObject(content);
	}
}
