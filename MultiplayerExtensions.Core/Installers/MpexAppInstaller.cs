using BeatSaverSharp;
using MultiplayerExtensions.Core.Objects;
using MultiplayerExtensions.Core.Packets;
using SiraUtil.Zenject;
using Zenject;

namespace MultiplayerExtensions.Core.Installers
{
    class MpexAppInstaller : Installer
    {
        private readonly BeatSaver _beatsaver;

        public MpexAppInstaller(
            BeatSaver beatsaver)
        {
            _beatsaver = beatsaver;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(new UBinder<Plugin, BeatSaver>(_beatsaver)).AsSingle();
            Container.Bind<MpexPacketSerializer>().ToSelf().AsSingle();
            Container.Bind<MpexLevelDownloader>().ToSelf().AsSingle();
        }
    }
}
