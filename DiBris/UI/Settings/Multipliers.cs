using Zenject;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI.Settings
{
    internal class Multipliers : Parseable
    {
        public override string Name => nameof(Multipliers);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Multipliers).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("lifetime")]
        protected float Lifetime
        {
            get => _config.LifetimeMultiplier;
            set => _config.LifetimeMultiplier = value;
        }

        [UIValue("velocity")]
        protected float Velocity
        {
            get => _config.VelocityMultiplier;
            set => _config.VelocityMultiplier = value;
        }

        [UIValue("gravity")]
        protected float Gravity
        {
            get => _config.GravityMultiplier;
            set => _config.GravityMultiplier = value;
        }

        [UIValue("rotation")]
        protected float Rotation
        {
            get => _config.RotationMultiplier;
            set => _config.RotationMultiplier = value;
        }

        [UIValue("scale")]
        protected float Scale
        {
            get => _config.Scale;
            set => _config.Scale = value;
        }

        [UIAction("percent-formatter")]
        protected string PercentFormatter(float value)
        {
            return value.ToString("P2");
        }

        [UIAction("multiplier-formatter")]
        protected string MultiplierFormatter(float value)
        {
            return $"{value:N}x";
        }
    }
}