using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RR.Network
{
	[Obsolete("Switch everything to urls with picLoader and remove this")]
	public class PNGLoader : MonoBehaviour
	{
		private static GameObject _imageLoader;

		public bool UseCaching = true;
		public bool ForceToSquare = true;
		public int Timeout = 30; // seconds

		[Header("PNG Loader")]
		public bool consoleLogging;
		[Tooltip("For images that need to be loaded on start function")]
		[SerializeField]
		protected bool loadOnStart;
		[Tooltip("The URL for the start loading")]
		[SerializeField]
		protected string _startLoadURL = @"https://...";

		private static readonly Dictionary<string, Stack<Promise<Texture2D>>> promises = new Dictionary<string, Stack<Promise<Texture2D>>>();
		private static readonly List<string> BrokenLinks = new List<string>();

		private static string GetFilePath(string url) => $"images/cache/{url.ToMd5Hash()}.png";
		private static bool IsInCache(string url) => RRFile.Exists(GetFilePath(url));
		private static Texture2D GetFromCache(string url) => RRFile.LoadTexture(GetFilePath(url));

		protected virtual void Awake()
		{
			if (!_imageLoader)
			{
				_imageLoader = new GameObject(nameof(PNGLoader));
				DontDestroyOnLoad(_imageLoader);
			}

			if (loadOnStart)
				LoadImage(_startLoadURL);
		}

		public static void Preload(Action onComplete = null, Action<Exception> onFailed = null, bool forceToSquare = true, params string[] urls)
		{
			try
			{
				var count = 0;
				foreach (var url in urls.Where(url => !IsInCache(url)))
				{
					if (!_imageLoader)
					{
						RRLogger.Error("_imageLoader is destroyed but trying to be accessed");
						continue;
					}

					RRLogger.Log($"Preloading {url}".Gray());

					var loader = _imageLoader.AddComponent<PNGLoader>();
					loader.ForceToSquare = forceToSquare;
					loader.LoadImage(url, texture =>
						{
							Destroy(loader);
							count++;

							if (count >= urls.Length)
								onComplete?.Invoke();
						});
				}
			}
			catch (Exception e)
			{
				onFailed?.Invoke(e);
			}
		}

		/// <summary>
		/// Downloads Image
		/// </summary>
		/// <param name="imageUrl"></param>
		/// <returns></returns>
		protected void LoadImage(string imageUrl, Action<Texture2D> onCompleted = null, Action<Exception> onFailed = null)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				RRLogger.Error($"Url is empty '{name}'");
				onFailed?.Invoke(new ArgumentNullException(nameof(imageUrl)));
				return;
			}

			LoadImageInternal(imageUrl, onCompleted, onFailed);
		}

		private void LoadImageInternal(string url, Action<Texture2D> onCompleted, Action<Exception> onFailed)
		{
			if (consoleLogging)
				RRLogger.Log(url);

			if (url.StartsWith("Resources/")|| url.StartsWith("resources/"))
			{
				var path = url
					.Replace("resources/", string.Empty)
					.Replace("Resources/", string.Empty);

				var texture2D = Resources.Load<Texture2D>(path);
				onCompleted(texture2D);
				return;
			}

			if (UseCaching && IsInCache(url))
			{
				onCompleted(GetFromCache(url));
				return;
			}

			if (BrokenLinks.Contains(url))
			{
				onFailed(new Exception($"URL[{url}] is in {nameof(BrokenLinks)} list."));
				return;
			}

			var promise = new Promise<Texture2D>(onCompleted, onFailed);

			// if url is already being called
			// add promise to stack it will be resolve later
			if (promises.ContainsKey(url))
			{
				promises[url].Push(promise);
				return;
			}

			promises[url] = new Stack<Promise<Texture2D>>();
			promises[url].Push(promise);

			var loader = _imageLoader.AddComponent<PNGLoader>();

			loader.StartCoroutine(loader.DownloadImage(url, t =>
			{
				promises[url].ForEach(p => p.Resolve(t));
				promises.Remove(url);

				if (UseCaching)
					RRFile.Save(GetFilePath(url), t.EncodeToPNG());

				Destroy(loader);
			}, exception =>
			{
				promises[url].ForEach(p => p.Reject(exception));
				promises[url].Clear();

				Destroy(loader);
			}));
		}

		private IEnumerator DownloadImage(string url, Action<Texture2D> callback, Action<Exception> onFailed)
		{
			var timeoutAttempts = 0;

			// create request
			var request = new UnityWebRequest(url)
			{
				timeout = Timeout,
				downloadHandler = new DownloadHandlerTexture()
			};

			while (!request.isDone || request.responseCode == 504 && timeoutAttempts <= 3)
			{
				if (timeoutAttempts++ > 0)
					RRLogger.Warning($"504 Timeout error. Retrying... [attempt: {timeoutAttempts}]");

				request = new UnityWebRequest(url)
				{
					timeout = Timeout,
					downloadHandler = new DownloadHandlerTexture()
				};

				yield return request.SendWebRequest();
			}

			// wait for request to be completed
			if (request.result != UnityWebRequest.Result.ProtocolError 
			    && request.result != UnityWebRequest.Result.ConnectionError)
			{
				var texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
				callback(texture);
				yield break;
			}

			if (request.result != UnityWebRequest.Result.ConnectionError && request.responseCode != 504)
				BrokenLinks.Add(url);

			onFailed(new Exception($"Response: {request.responseCode.ToString().Bold().Orange()}\nURL: {url.Ellipsis(60)}\n" +
				$"Image on [{gameObject.name}] failed to load after {timeoutAttempts.ToString().Bold()} attempts.\n{request.error.Grey()}."));
		}

		private IEnumerator ImageHeadRequest(string url, Promise<bool> promise)
		{
			RRLogger.Log($"Checking if image url: {url}");

			var request = UnityWebRequest.Head(url);
			//request.SetRequestHeader("Accept-Language", i18n.CurrentLanguage);
			yield return request.SendWebRequest();

			var headers = request.GetResponseHeaders();

			if (request.result == UnityWebRequest.Result.ProtocolError 
			    || request.result == UnityWebRequest.Result.ConnectionError)
			{
				promise.OnFailed(new Exception(request.error));
				yield break;
			}

			if (headers.TryGetValue("Content-Type", out var contentType))
			{
				var isPng = contentType == "image/png";
				var isJpeg = contentType == "image/jpeg";
				var isGif = contentType == "image/gif";
				promise.OnSuccess(isPng || isJpeg || isGif);
				yield break;
			}

			promise.OnFailed(new ArgumentException("'Content-Type' header could not be found"));
		}
	}
}
