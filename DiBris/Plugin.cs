using IPA;
using SiraUtil;
using SiraUtil.Zenject;
using IPA.Config.Stores;
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