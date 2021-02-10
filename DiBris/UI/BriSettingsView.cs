using HMUI;
using Zenject;
using UnityEngine;
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
        protected Config _config = null!;
        protected UIParser _uiParser = null!;
        protected ProfileManager _profileManager = null!;

        [UIParams]
        protected readonly BSMLParserParams parserParams = null!;

        [UIComponent("title-bar")]
        protected readonly ImageView titleBar = null!;

        [UIComponent("tab-selector")]
        protected readonly TabSelector tabSelector = null!;

        [UIValue("setting-windows")]
        protected readonly List<object> settingWindows = new List<object>();

        [Inject]
        protected void Construct(Config config, UIParser uiParser, ProfileManager profileManager)
        {
            _config = config;
            _uiParser = uiParser;
            _profileManager = profileManager;
            settingWindows.Add(new General());
            settingWindows.Add(new Multipliers());
            settingWindows.Add(new Positioning());
            settingWindows.Add(new Conditions());
            settingWindows.Add(new Miscellaneous());
        }

        [UIAction("#post-parse")]
        protected async Task Parsed()
        {
            titleBar.color0 = Color.white;
            titleBar.color1 = Color.white.ColorWithAlpha(0);
            titleBar.SetAllDirty();
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

        private async void SelectedCell(SegmentedControl _, int index)
        {
            if (settingWindows[index] is Parseable parseable)
            {
                await _uiParser.Parse(parseable);
            }
        }

        protected override async void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            await _profileManager.AllSubProfiles();
            _profileManager.Save(_config);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            tabSelector.textSegmentedControl.didSelectCellEvent -= SelectedCell;
        }
    }
}