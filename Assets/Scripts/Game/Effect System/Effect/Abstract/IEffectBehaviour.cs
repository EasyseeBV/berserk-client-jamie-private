using Game.Entities;

namespace Game.Effect_System.Effects
{
	public interface IEffectBehaviour
	{
		void Fill(PickInfo pickInfo);

		void Perform(IInteractiveEntity[] targets);

		bool StepForward();
		
		void Cancel();
	}
}