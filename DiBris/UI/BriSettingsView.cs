using Zenject;
using DiBris.Managers;
using DiBris.UI.Settings;
using System.Threading.Tasks;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.settings-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\settings-view.bsml")]
    internal class BriSettingsView : BSMLAutomaticViewController
    {
        protected UIParser _uiParser = null!;

        [UIParams]
        protected readonly BSMLParserParams parserParams = null!;

        [UIComponent("tab-selector")]
        protected readonly TabSelector tabSelector = null!;

        [UIValue("setting-windows")]
        protected readonly List<object> settingWindows = new List<object>();

        [Inject]
        protected void Construct(UIParser uiParser)
        {
            _uiParser = uiParser;
            settingWindows.Add(new General());
            settingWindows.Add(new Multipliers());
            settingWindows.Add(new Positioning());
            settingWindows.Add(new Conditions());
        }

        [UIAction("#post-parse")]
        protected async Task Parsed()
        {
            if (settingWindows[0] is Parseable parseable)
            {
                await _uiParser.Parse(parseable);
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            tabSelector.textSegmentedControl.didSelectCellEvent += SelectedCell;

            parserParams.EmitEvent("refresh");
            foreach (IRefreshable refreshable in settingWindows)
                refreshable.Refresh();
        }

        private async void SelectedCell(HMUI.SegmentedControl _, int index)
        {
            if (settingWindows[index] is Parseable parseable)
            {
                await _uiParser.Parse(parseable);
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            tabSelector.textSegmentedControl.didSelectCellEvent -= SelectedCell;
        }
    }
}