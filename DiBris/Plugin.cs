using IPA;
using SiraUtil;
using SiraUtil.Zenject;
using IPA.Config.Stores;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;
using DiBris.Components;
using IPA.Utilities;

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
                    if (spawner.GetType() == typeof(NoteDebrisSpawner))
                    {
                        var diSpawner = spawner.Upgrade<NoteDebrisSpawner, DiSpawner>();
                        var effectSpawner = ctx.GetInjected<NoteCutCoreEffectsSpawner>();
                        ReflectionUtil.SetField<NoteCutCoreEffectsSpawner, NoteDebrisSpawner>(effectSpawner, "_noteDebrisSpawner", diSpawner);
                        ctx.Container.QueueForInject(diSpawner);
                        ctx.AddInjectable(diSpawner);
                    }
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