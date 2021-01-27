using Zenject;
using System.ComponentModel;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI.Settings
{
    internal class Miscellaneous : Parseable, INotifyPropertyChanged
    {
        public override string Name => nameof(Miscellaneous);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Miscellaneous).ToLower()}.bsml";

        public event PropertyChangedEventHandler? PropertyChanged;

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("fixate-rotation")]
        protected bool FixateRotation
        {
            get => _config.FixateRotationToZero;
            set => _config.FixateRotationToZero = value;
        }

        [UIValue("fixate-zpos")]
        protected bool FixateZPos
        {
            get => _config.FixateZPos;
            set => _config.FixateZPos = value;
        }

        [UIValue("do-fixed-lifetime")]
        protected bool DoFixedLifetime
        {
            get => _config.FixedLifetime;
            set => _config.FixedLifetime = value;
        }

        [UIValue("fixed-lifetime")]
        protected float FixedLifetime
        {
            get => _config.FixedLifetimeLength;
            set => _config.FixedLifetimeLength = value;
        }

        [UIValue("do-grid-snap")]
        protected bool DoGridSnap
        {
            get => _config.SnapToGrid;
            set => _config.SnapToGrid = value;
        }

        [UIValue("grid-scale")]
        protected float GridScale
        {
            get => _config.GridScale;
            set => _config.GridScale = value;
        }

        [UIAction("hidden-prop-change")]
        protected async Task HiddenPropertyChanged(bool _)
        {
            await SiraUtil.Utilities.AwaitSleep(0);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoGridSnap)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoFixedLifetime)));
        }

        [UIAction("time-formatter")]
        protected string TimeFormatter(float value) => $"{value:0.00} seconds";
    }
}