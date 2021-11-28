using MultiplayerExtensions.Core.Objects;
using Zenject;

namespace MultiplayerExtensions.Core.Installers
{
    class MpexMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Inject(Container.Resolve<NetworkPlayerEntitlementChecker>());
        }
    }
}
