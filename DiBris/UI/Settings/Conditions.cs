using System;
using Zenject;
using System.Linq;
using DiBris.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI.Settings
{
    internal class Conditions : Parseable, INotifyPropertyChanged
    {
        public override string Name => nameof(Conditions);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(Conditions).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        public event PropertyChangedEventHandler? PropertyChanged;

        [UIValue("on-njs")]
        protected bool OnNJS
        {
            get => _config.Parameters.DoNJS;
            set => _config.Parameters.DoNJS = value;
        }

        [UIValue("on-nps")]
        protected bool OnNPS
        {
            get => _config.Parameters.DoNPS;
            set => _config.Parameters.DoNPS = value;
        }

        [UIValue("on-length")]
        protected bool OnLength
        {
            get => _config.Parameters.DoLength;
            set => _config.Parameters.DoLength = value;
        }

        [UIValue("njs")]
        protected float NJS
        {
            get => _config.Parameters.NJS;
            set => _config.Parameters.NJS = value;
        }

        [UIValue("nps")]
        protected float NPS
        {
            get => _config.Parameters.NPS;
            set => _config.Parameters.NPS = value;
        }

        [UIValue("length")]
        protected float Length
        {
            get => _config.Parameters.Length;
            set => _config.Parameters.Length = value;
        }

        [UIValue("mode")]
        protected DisableMode Mode
        {
            get => _config.Parameters.Mode;
            set => _config.Parameters.Mode = value;
        }

        [UIValue("condition-types")]
        protected List<object> Sensitivities => ((DisableMode[])Enum.GetValues(typeof(DisableMode))).Cast<object>().ToList();

        [UIAction("hidden-prop-change")]
        protected async Task HiddenPropertyChanged(bool _)
        {
            await SiraUtil.Utilities.AwaitSleep(0);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnNJS)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnNPS)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnLength)));
        }

        [UIAction("njs-formatter")]
        protected string NJSFormatter(float value) => $"{value:N} NJS";

        [UIAction("nps-formatter")]
        protected string NPSFormatter(float value) => $"{value:N} NPS";

        [UIAction("time-formatter")]
        protected string TimeFormatter(float value) => $"{(int)value} seconds";

    }
}