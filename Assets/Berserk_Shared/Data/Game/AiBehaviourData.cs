using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Game
{
	public class AiBehaviourData : IConfigData
	{
		public string Id { get; set; }
		public AiNode Node { get; }

		[JsonConstructor]
		public AiBehaviourData(string id, AiNode node)
		{
			Id = id;
			Node = node;
		}

		public AiBehaviourData()
		{
		}
	}
}