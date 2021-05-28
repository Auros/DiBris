using IPA;
using SiraUtil;
using DiBris.UI;
using IPA.Loader;
using IPA.Utilities;
using DiBris.Managers;
using SiraUtil.Zenject;
using DiBris.Components;
using IPA.Config.Stores;
using SiraUtil.Attributes;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace DiBris
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        internal static readonly FieldAccessor<NoteCutCoreEffectsSpawner, NoteDebrisSpawner>.Accessor DebrisSpawner = FieldAccessor<NoteCutCoreEffectsSpawner, NoteDebrisSpawner>.GetAccessor("_noteDebrisSpawner");

        [Init]
        public Plugin(Conf conf, IPALogger log, Zenjector zenjector, PluginMetadata metadata)
        {
            var config = conf.Generated<Config>();
            config.Version = metadata.Version;

            zenjector
                .On<PCAppInit>()
                .Pseudo(Container =>
                {
                    Container.BindInstance(metadata).WithId(nameof(DiBris)).AsCached();
                    Container.Bind<ProfileManager>().AsSingle();
                    Container.BindInstance(config).AsSingle();
                    Container.BindLoggerAsSiraLogger(log);
                });

            zenjector
                .On<MainSettingsMenuViewControllersInstaller>()
                .Pseudo(Container =>
                {
                    Container.Bind<UIParser>().AsSingle();
                    Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
                    Container.Bind<BriMainView>().FromNewComponentAsViewController().AsSingle();
                    Container.Bind<BriInfoView>().FromNewComponentAsViewController().AsSingle();
                    Container.Bind<BriProfileView>().FromNewComponentAsViewController().AsSingle();
                    Container.Bind<BriSettingsView>().FromNewComponentAsViewController().AsSingle();
                    Container.Bind<BriFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(BriFlowCoordinator)).AsSingle();
                });

            zenjector
                .On<GameplayCoreInstaller>()
                .Pseudo((_) => { })
                .Mutate<NoteDebrisSpawner>((ctx, spawner) =>
                {
                    var diSpawner = spawner.Upgrade<NoteDebrisSpawner, DiSpawner>();
                    var effectSpawner = ctx.GetInjected<NoteCutCoreEffectsSpawner>();
                    DebrisSpawner(ref effectSpawner) = diSpawner;
                    ctx.Container.QueueForInject(diSpawner);
                    ctx.AddInjectable(diSpawner);
                });
        }

        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}