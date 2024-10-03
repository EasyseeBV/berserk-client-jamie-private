using System;
using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialHintUnmaskEntity : IDisposable
	{
		ITutorialUnmaskData Data { get; }

		ITutorialHintTarget Target { get; }

		void Refresh();
	}
}