using IPA;
using SiraUtil;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace DiBris
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger log, Zenjector zenjector)
        {
            zenjector
                .On<PCAppInit>()
                .Pseudo(Container => Container.BindLoggerAsSiraLogger(log,
#if DEBUG
                true
#else
                false
#endif
                ));
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