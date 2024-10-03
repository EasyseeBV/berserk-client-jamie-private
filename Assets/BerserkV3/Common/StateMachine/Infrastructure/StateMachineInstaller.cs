using Zenject;

namespace BerserkV3.Common.StateMachine
{
    public class StateMachineInstaller : Installer<StateMachineInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<StateMachine>()
                .AsTransient();
        }
    }
}