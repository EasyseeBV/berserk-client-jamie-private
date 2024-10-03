using RR.Network.Rest;
using Sirenix.Utilities;

namespace BerserkV3.Common.Network
{
	public static class RestExtensions
	{
		public static string GetMessage<T>(this APIResponse<T> response) where T : class
		{
			if (response.Code == 0)
				return "Timeout exceeded. Please try again.";
			
			if ((int) response.Code > 499)
				return "Something went wrong. Please try again or make a report.";
			
			var message = string.IsNullOrWhiteSpace(response.RawMessage) 
				? response.ErrorMessage 
				: response.RawMessage;
			
			if (message.ToLower().Trim().Contains("credentials"))
				return "Invalid Credentials.";

			if (string.IsNullOrWhiteSpace(message))
				return "Response message is empty.";
			
			return message;
		}
	}
}