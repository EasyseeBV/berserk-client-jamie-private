using System.Threading.Tasks;

namespace BerserkV3.Startup.Abstractions
{
	public interface IMessageApplication
	{
		Task Info(string message, string okText = null, string title = null);
		Task Critial(string mesage = null, string okText = null, string title = null);
	}
}