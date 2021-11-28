using MultiplayerExtensions.Core.Objects;
using Zenject;

namespace MultiplayerExtensions.Core.Installers
{
    class MpexMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Inject sira stuff that didn't get injected on appinit
            Container.Inject(Container.Resolve<NetworkPlayerEntitlementChecker>());
        }
    }
}
