using Berserk.Shared.SignalR.Abstractions;
using Newtonsoft.Json;

namespace Berserk.Shared.SignalR.Common
{
	public class ErrorModel : SocketResponseBaseModel
	{
		public string Message { get; protected set; }
		public int Code { get; }

		public ErrorModel(int code)
		{
			Code = code;
		}

		[JsonConstructor]
		public ErrorModel(int code, string message)
		{
			Code = code;
			Message = message;
		}
	}
}