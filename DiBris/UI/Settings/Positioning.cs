using Zenject;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI.Settings
{
    internal class Positioning : Parseable
    {
        public override string Name => nameof(Positioning);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Positioning).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("pos-offset-x")]
        protected float PosOffsetX
        {
            get => _config.AbsolutePositionOffset.x;
            set
            {
                _config.AbsolutePositionOffsetX = value;
            }
        }

        [UIValue("pos-offset-y")]
        protected float PosOffsetY
        {
            get => _config.AbsolutePositionOffset.y;
            set => _config.AbsolutePositionOffsetY = value;
        }

        [UIValue("pos-offset-z")]
        protected float PosOffsetZ
        {
            get => _config.AbsolutePositionOffset.z;
            set => _config.AbsolutePositionOffsetZ = value;
        }

        [UIValue("pos-scale")]
        protected float PosScale
        {
            get => _config.AbsolutePositionScale;
            set => _config.AbsolutePositionScale = value;
        }

        [UIAction("percent-formatter")]
        protected string PercentFormatter(float value)
        {
            return value.ToString("P2");
        }

        [UIAction("length-formatter")]
        protected string LengthFormatter(float value)
        {
            return $"{value:N} m";
        }
    }
}