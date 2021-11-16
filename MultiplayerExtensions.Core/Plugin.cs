using BeatSaverSharp;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Loader;
using IPA.Logging;
using MultiplayerExtensions.Core.Installers;
using SiraUtil.Zenject;
using System;
using IPALogger = IPA.Logging.Logger;

namespace MultiplayerExtensions.Core
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    class Plugin
    {
        private readonly Harmony _harmony;
        private readonly PluginMetadata _metadata;
		private readonly BeatSaver _beatsaver;
        public const string ID = "com.goobwabber.multiplayerextensions";

		[Init]
		public Plugin(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenjector)
		{
			_harmony = new Harmony(ID);
			_metadata = pluginMetadata;
			_beatsaver = new BeatSaver(ID, new Version(_metadata.HVersion.ToString()));

			zenjector.UseMetadataBinder<Plugin>();
			zenjector.UseLogger(logger);
			zenjector.UseHttpService();
			zenjector.UseSiraSync(SiraUtil.Web.SiraSync.SiraSyncServiceType.GitHub, "Goobwabber", "MultiplayerExtensions");
			zenjector.Install<MpexAppInstaller>(Location.App, _beatsaver);
		}

		[OnEnable]
		public void OnEnable()
		{
			_harmony.PatchAll(_metadata.Assembly);
		}

		[OnDisable]
		public void OnDisable()
		{
			_harmony.UnpatchAll(ID);
		}
	}
}
