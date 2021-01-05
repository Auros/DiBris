using Zenject;
using System.IO;
using IPA.Loader;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.info-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\info-view.bsml")]
    internal class BriInfoView : BSMLAutomaticViewController
    {
        [Inject(Id = nameof(DiBris))]
        protected readonly PluginMetadata _pluginMetadata = null!;

        [Inject]
        protected readonly IPlatformUserModel _platformUserModel = null!;

        private string _infoText = "Loading...";
        [UIValue("info-text")]
        protected string InfoText
        {
            get => _infoText;
            set
            {
                _infoText = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("#post-parse")]
        protected async Task Parsed()
        {
            // Load Text Asset (Asynchronously)
            Stream stream = _pluginMetadata.Assembly.GetManifestResourceStream($"{nameof(DiBris)}.Resources.info.txt");
            StreamReader sr = new StreamReader(stream);
            string text = await sr.ReadToEndAsync();
            sr.Dispose();
            stream.Dispose();

            var user = await _platformUserModel.GetUserInfo();
            switch (user.platformUserId)
            {
                case "76561198064659288":
                    text += "hi denyah";
                    break;
                case "76561198055583703":
                    text += "we are back? back from where?";
                    break;
                case "76561198035698451":
                    text += "dibris? i hardly even know her!";
                    break;
            }
            InfoText = text;
        }
    }
}