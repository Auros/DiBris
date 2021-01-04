using IPA;
using SiraUtil;
using IPA.Utilities;
using SiraUtil.Zenject;
using IPA.Config.Stores;
using DiBris.Components;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace DiBris
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(Conf conf, IPALogger log, Zenjector zenjector)
        {
            var config = conf.Generated<Config>();
            zenjector
                .On<PCAppInit>()
                .Pseudo(Container =>
                {
                    Container.BindInstance(config).AsSingle();
                    Container.BindLoggerAsSiraLogger(log,
#if DEBUG
                true
#else
                false
#endif
                );
                });

            zenjector
                .On<GameplayCoreInstaller>()
                .Pseudo((_) => { })
                .Mutate<NoteDebrisSpawner>((ctx, spawner) =>
                {
                    var diSpawner = spawner.Upgrade<NoteDebrisSpawner, DiSpawner>();
                    var effectSpawner = ctx.GetInjected<NoteCutCoreEffectsSpawner>();
                    ReflectionUtil.SetField<NoteCutCoreEffectsSpawner, NoteDebrisSpawner>(effectSpawner, "_noteDebrisSpawner", diSpawner);
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