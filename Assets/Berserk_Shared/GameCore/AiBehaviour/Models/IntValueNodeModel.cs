namespace Berserk.Shared.GameCore.AiBehaviour.Models
{
	[System.Serializable]
	public class IntValueNodeModel
	{
		public int Value;
		
		public override string ToString()
		{
			return "RandomNodeModel: " + Value;
		}
	}
}