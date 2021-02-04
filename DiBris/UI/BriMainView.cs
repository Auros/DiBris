using HMUI;
using System;
using Zenject;
using Tweening;
using IPA.Loader;
using UnityEngine;
using System.Linq;
using DiBris.Managers;
using System.Threading.Tasks;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.main-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\main-view.bsml")]
    internal class BriMainView : BSMLAutomaticViewController
    {
        #region Injections

        [Inject]
        protected readonly TweeningManager _tweeningManager = null!;

        [Inject]
        protected readonly ProfileManager _profileManager = null!;

        [Inject(Id = nameof(DiBris))]
        protected readonly PluginMetadata pluginMetadata = null!;

        [Inject]
        protected readonly Config _config = null!;

        [UIComponent("desc-text")]
        protected CurvedTextMeshPro descText = null!;

        [UIComponent("button-grid")]
        protected RectTransform buttonGrid = null!;

        [UIComponent("logo-image")]
        protected ImageView logoImage = null!;

        [UIValue("version")]
        protected string Version => $"v{pluginMetadata.Version}";

        [UIParams]
        protected BSMLParserParams parserParams = null!;

        #endregion

        internal Material? noGlowMatRound;
        public event Action<BriFlowCoordinator.NavigationEvent>? EventNavigated;
        private readonly List<NoTransitionsButton> buttons = new List<NoTransitionsButton>();
        private readonly List<TextTransitioner> _textTransitioners = new List<TextTransitioner>();

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            if (firstActivation)
                buttons.AddRange(buttonGrid.GetComponentsInChildren<NoTransitionsButton>(true));
            descText.alpha = 0f;
            _textTransitioners.Clear();
            _textTransitioners.Add(new TextTransitioner("Learn more about this mod", buttons[0]));
            _textTransitioners.Add(new TextTransitioner("Create, switch, and remove your config profile(s)", buttons[1]));
            _textTransitioners.Add(new TextTransitioner("Edit the settings for the current profile", buttons[2]));
            _textTransitioners.Add(new TextTransitioner("Donate to the mod creator (opens in browser)", buttons[3]));
            _textTransitioners.Add(new TextTransitioner("Open the GitHub page (in your browser)", buttons[4]));
            _textTransitioners.Add(new TextTransitioner("Reset all your settings.", buttons[5]));
            foreach (var transitioner in _textTransitioners)
                transitioner.StateChanged += ButtonSelectionStateChanged;

            if (firstActivation || noGlowMatRound == null)
            {
                // Yes. It was either this or recursively dig through 3 object. Will be making an API to expose things like this easier in the future.
                noGlowMatRound = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlowRoundEdge").First();
                logoImage.material = noGlowMatRound;
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

            foreach (var transitioner in _textTransitioners)
            {
                transitioner.StateChanged -= ButtonSelectionStateChanged;
                transitioner.Dispose();
            }
            _textTransitioners.Clear();
        }

        private void ButtonSelectionStateChanged(string descriptionText, NoTransitionsButton.SelectionState selectionState)
        {
            var to = selectionState == NoTransitionsButton.SelectionState.Normal ? 0f : 1f;
            _tweeningManager.KillAllTweens(this);
            descText.text = descriptionText;
            var from = descText.alpha;

            _tweeningManager.AddTween(new FloatTween(from, to, value =>
            {
                descText.alpha = value;
            }, 0.2f, EaseType.InSine, 0.1f), this);
        }

        [UIAction("clicked-info-button")] protected void ClickedInfoButton() => EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Info);
        [UIAction("clicked-github-button")] protected void ClickedGithubButton() => Application.OpenURL("https://github.com/Auros/DiBris");
        [UIAction("clicked-donate-button")] protected void ClickedDonateButton() => Application.OpenURL("https://ko-fi.com/aurosnex");
        [UIAction("clicked-profile-button")] protected void ClickedProfileButton() => EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Profile);
        [UIAction("clicked-settings-button")] protected void ClickedSettingsButton() => EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Settings);

        [UIAction("reset")]
        protected async Task Reset()
        {
            var allSubProfiles = await _profileManager.AllSubProfiles();
            foreach (var profile in allSubProfiles)
            {
                _profileManager.Delete(profile);
            }

            var version = _config.Version;
            _config.CopyFrom(new Config
            {
                Version = version
            });
            _config.Save();
            _config.Changed();
            parserParams.EmitEvent("hide-reset");
            EventNavigated?.Invoke(BriFlowCoordinator.NavigationEvent.Reset);
        }

        private class TextTransitioner : IDisposable
        {
            private readonly string _text;
            private readonly NoTransitionsButton _button;
            public event Action<string, NoTransitionsButton.SelectionState>? StateChanged;

            public TextTransitioner(string text, NoTransitionsButton button)
            {
                _text = text;
                _button = button;
                button.selectionStateDidChangeEvent += SelectionDidChange;
            }

            private void SelectionDidChange(NoTransitionsButton.SelectionState state)
            {
                StateChanged?.Invoke(_text, state);
            }

            public void Dispose()
            {
                _button.selectionStateDidChangeEvent -= SelectionDidChange;
            }
        }
    }
}