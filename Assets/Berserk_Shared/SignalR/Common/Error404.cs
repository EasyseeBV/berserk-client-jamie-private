using Newtonsoft.Json;

namespace Berserk.Shared.SignalR.Common
{
	public class Error404 : ErrorModel
	{
		public Error404() : base(404)
		{
			Message = "Not found.";
		}

		[JsonConstructor]
		public Error404(string message) : base(404, message)
		{
			if (string.IsNullOrEmpty(message))
				Message = "Not found.";
		}
	}
}