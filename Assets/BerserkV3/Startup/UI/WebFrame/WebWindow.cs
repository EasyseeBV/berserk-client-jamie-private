using System;
using BerserkV3.Common.EventSource;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.UIService;
using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView;
using Zenject;

namespace BerserkV3.Startup.UI
{
	public readonly struct ViewClosedEvent { }
		
	public readonly struct UrlChangedEvent
	{
		public UrlChangedEventArgs NewUrl { get; }

		public UrlChangedEvent(UrlChangedEventArgs newUrl)
		{
			NewUrl = newUrl;
		}
	}
	
	public class WebWindow : UIWindowBase
	{
		private const string BLANK_URL = "about:blank";
		[SerializeField] private CanvasWebViewPrefab webViewPrefab;
		[SerializeField] private RectTransform webLayout;
		[SerializeField] private Button closeButton;
		
	    private event Action OnCloseRequested;
	    private IWebView WebView => webViewInstance ? webViewInstance.WebView : null;
	    private bool Initialized => WebView is {IsInitialized: true};
	    
	    private IEventPublisher eventPublisher;
		private CanvasWebViewPrefab webViewInstance;
		private string linkToOpen;
		
		[Inject]
		public void Construct(IEventPublisher eventPublisher)
		{
			this.eventPublisher = eventPublisher;
		}
		
		/// <summary>
		/// Clean previous using & data and instantiate a new instance of WebView and prepare to init.
		/// </summary>
		/// <param name="isMobileView">affect on resolution</param>
		/// <param name="url">loading default page after init.</param>
	    public async UniTask SetupAsync(bool isMobileView, string url = BLANK_URL)
		{	
			try
			{
				await ClearAllDataAsync();
				
				linkToOpen = url;
				webViewInstance = Instantiate(webViewPrefab, webLayout);
				webViewInstance.Resolution = WebHelper.GetResolutionScale(Application.isMobilePlatform);
				Web.SetUserAgent(WebHelper.GetUserAgent(isMobileView));
				Web.SetIgnoreCertificateErrors(true);
				// TODO do not wait init here : WaitInitializationAsync, because internal async logic based on coroutines
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

	    public async UniTask OpenUrlAsync(string url)
	    {
		    try
		    {
			    await WaitInitializationAsync();
			    WebView.LoadUrl(url);
		    }
		    catch (Exception e)
		    {
			    RRLogger.Error(e);
		    }
	    }
		
	    public void SetCloseAction(Action value)
	    {
		    closeButton.onClick.AddListener(() => value?.Invoke());
	    }
		
	    public void Blank()
	    {
		    if (!Initialized) 
			    throw new InvalidOperationException($"Call {nameof(SetupAsync)} before display the window.");
			
		    WebView.StopLoad();
		    WebView.LoadUrl("about:blank");
	    }

		public override async void Show() // before show subscribe and start load a page
		{
			await WaitInitializationAsync(); // initialisation is possible only when the window is enabled in the game view mode.
			if (!Initialized)
				throw new InvalidOperationException($"Call {nameof(SetupAsync)} before display the window.");

			WebView.CloseRequested += WebViewOnCloseRequested;
			WebView.UrlChanged += WebViewOnUrlChanged;
			WebView.LoadProgressChanged += WebViewOnLoadProgressChanged;
			WebView.Terminated += WebViewOnTerminated;
			WebView.LoadFailed += WebViewOnPageLoadFailed;
			
			OpenUrlAsync(linkToOpen).Forget();
		}

		public override void Hide() // Before hidden unsubscribe and stop load a page
		{
			if (Initialized)
			{
				WebView.CloseRequested -= WebViewOnCloseRequested;
				WebView.UrlChanged -= WebViewOnUrlChanged;
				WebView.LoadFailed -= WebViewOnPageLoadFailed;
				WebView.LoadProgressChanged -= WebViewOnLoadProgressChanged;
				WebView.Terminated -= WebViewOnTerminated;
				WebView.StopLoad();
			}
			
			closeButton.onClick.RemoveAllListeners();
			OnCloseRequested = null;
		}
		
		private async UniTask WaitInitializationAsync()
		{
			if (!webViewInstance)
				throw new InvalidOperationException("Web view is not ready to initialize.");
			
			await webViewInstance.WaitUntilInitialized();
			await UniTask.WaitUntil(() => Initialized || !Application.isPlaying);
		}
		
		private async UniTask ClearAllDataAsync()
		{
			linkToOpen = BLANK_URL;
			if (webViewInstance) 
			{
				// 1. Destroy all the webviews in the application.
				webViewInstance.Destroy();
				webViewInstance = null;
				await UniTask.Delay(300);
			}
			
#if UNITY_STANDALONE || UNITY_EDITOR
			// 2. Terminate Chromium.
			await StandaloneWebView.TerminateBrowserProcess();
#endif
			var internalInit = false;
        	while (Application.isPlaying && !internalInit)
        	{
        		try
        		{
	                // 3. Call the API that can't be called while Chromium is running.
	                await UniTask.Delay(300); // wait before all I/O ended
					Web.ClearAllData();
        			internalInit = true;
        		}
        		catch (Exception e)
        		{
        			RRLogger.Error(e);
        		}
        	}
		}

		#region Callbacks
		private void WebViewOnUrlChanged(object sender, UrlChangedEventArgs e)
		{
			eventPublisher.Publish(new UrlChangedEvent(e));
		}

		private void WebViewOnCloseRequested(object sender, EventArgs e)
		{
			eventPublisher.Publish(new ViewClosedEvent());
			OnCloseRequested?.Invoke();
		}
		
		private void WebViewOnTerminated(object sender, EventArgs e)
		{
			
		}
		
		private void WebViewOnLoadProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			
		}

		private void WebViewOnPageLoadFailed(object sender, EventArgs e)
		{
			
		}
		#endregion

	}
}