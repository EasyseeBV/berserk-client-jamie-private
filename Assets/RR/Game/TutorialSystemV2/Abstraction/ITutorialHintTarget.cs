using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialHintTarget : ITutorialHandler
	{
		Transform Transform { get; }
		
		TutorialVector3 Position { get; }

		TutorialVector3 Size { get; }
		TutorialVector3 SizeScale { get; set; }
	}
}