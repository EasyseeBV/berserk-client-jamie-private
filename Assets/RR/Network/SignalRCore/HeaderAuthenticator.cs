using BestHTTP;
using BestHTTP.Connections;
using BestHTTP.SignalRCore;
using System;

namespace RR.Network.SignalRCore
{
	public class HeaderAuthenticator : IAuthenticationProvider
	{
		public string Token { get; private set; }
		public HubConnection Connection { get; private set; }

		public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded = default;
		public event OnAuthenticationFailedDelegate OnAuthenticationFailed = default;

		public HeaderAuthenticator(string token, HubConnection connection)
		{
			Token = token;
			Connection = connection;
		}

		public bool IsPreAuthRequired => false;

		public void StartAuthentication() { }

		public void Cancel() { }

		public void PrepareRequest(HTTPRequest request)
		{
			if (HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) != SupportedProtocols.HTTP)
			{
				if (HTTPProtocolFactory.GetProtocolFromUri(request.Uri) != SupportedProtocols.WebSocket)
					request.Uri = PrepareUriImpl(request.Uri);
				return;
			}

			var negotiationToken = Connection?.NegotiationResult?.AccessToken;
			request.SetHeader("Authorization", !string.IsNullOrEmpty(negotiationToken)
				? $"Bearer {negotiationToken}" : $"Bearer {Token}");
		}

		public Uri PrepareUri(Uri uri)
		{
			if (!uri.Query.StartsWith("??")) 
				return PrepareUriImpl(uri);

			var builder = new UriBuilder(uri);
			builder.Query = builder.Query.Substring(2);

			return builder.Uri;

		}

		private Uri PrepareUriImpl(Uri uri)
		{
			var negotiationToken = Connection?.NegotiationResult?.AccessToken;
			var token = string.IsNullOrEmpty(negotiationToken) ? Token : negotiationToken;
			if (string.IsNullOrEmpty(token))
				return uri;

			if (!string.IsNullOrEmpty(uri.Query) &&
				uri.Query.IndexOf("access_token=", StringComparison.OrdinalIgnoreCase) >= 0)
				return uri;

			var query = string.IsNullOrEmpty(uri.Query) ? "" : uri.Query + "&";
			var uriBuilder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, query + "access_token=" + token);
			return uriBuilder.Uri;
		}

		protected virtual void OnOnAuthenticationFailed(IAuthenticationProvider provider, string reason) 
			=> OnAuthenticationFailed?.Invoke(provider, reason);

		protected virtual void OnOnAuthenticationSucceeded(IAuthenticationProvider provider) 
			=> OnAuthenticationSucceded?.Invoke(provider);
	}
}
