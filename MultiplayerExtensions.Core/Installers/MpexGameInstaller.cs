using MultiplayerExtensions.Core.Patchers;
using Zenject;

namespace MultiplayerExtensions.Core.Installers
{
    class MpexGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<IntroAnimationPatcher>().AsSingle();
        }
    }
}
