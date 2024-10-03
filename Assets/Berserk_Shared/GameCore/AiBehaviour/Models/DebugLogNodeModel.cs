namespace Berserk.Shared.GameCore.AiBehaviour.Models
{
	[System.Serializable]
	public class DebugLogNodeModel
	{
		public string Message;

		public override string ToString()
		{
			return "DebugLogNodeModel: " + Message;
		}
	}
}