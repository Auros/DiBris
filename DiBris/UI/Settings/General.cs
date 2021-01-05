using Zenject;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI.Settings
{
    internal class General : Parseable
    {
        public override string Name => nameof(General);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(General).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [UIValue("profile-name")]
        protected string ProfileName
        {
            get => _config.Name;
            set => _config.Name = value;
        }

        [UIValue("remove-all-debris")]
        protected bool RemoveAllDebris
        {
            get => _config.RemoveDebris;
            set => _config.RemoveDebris = value;
        }
    }
}