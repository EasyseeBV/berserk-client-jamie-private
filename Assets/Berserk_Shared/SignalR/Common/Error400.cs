using Newtonsoft.Json;

namespace Berserk.Shared.SignalR.Common
{
	public class Error400 : ErrorModel
	{
		public Error400() : base(400)
		{
			Message = "Bad request.";
		}

		[JsonConstructor]
		public Error400(string message) : base(404, message)
		{
			if (string.IsNullOrEmpty(message))
				Message = "Bad request.";
		}
	}
}