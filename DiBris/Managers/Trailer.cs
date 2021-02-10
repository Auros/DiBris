using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zenject;

namespace DiBris.Managers
{
    internal class Trailer : IInitializable, ITickable
    {
        [Inject]
        private readonly AudioTimeSyncController timeSync = null!;

        [Inject]
        private readonly Config config = null!;
        
        private bool _done;

        public async void Initialize()
        {
            await SiraUtil.Utilities.AwaitSleep(500);
            config.RemoveDebris = false;
            config.VelocityMultiplier = 1f;
            config.GravityMultiplier = 1f;
            config.LifetimeMultiplier = 1f;
        }

        public void Tick()
        {
            if (timeSync.songTime >= 22f && !_done)
            {

                config.VelocityMultiplier = 0f;
                config.GravityMultiplier = 0f;
                config.LifetimeMultiplier = 10f;
                _done = true;
            }
        }
    }
}
