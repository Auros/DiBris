using HMUI;
using System;
using Zenject;
using Tweening;
using UnityEngine;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.main-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\main-view.bsml")]
    internal class BriMainView : BSMLAutomaticViewController
    {
        [Inject]
        protected readonly TweeningManager _tweeningManager = null!;

        [UIComponent("button-grid")]
        protected RectTransform buttonGrid = null!;

        [UIComponent("desc-text")]
        protected CurvedTextMeshPro descText = null!;

        private readonly List<NoTransitionsButton> buttons = new List<NoTransitionsButton>();
        private readonly List<TextTransitioner> _textTransitioners = new List<TextTransitioner>();

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            if (firstActivation)
                buttons.AddRange(buttonGrid.GetComponentsInChildren<NoTransitionsButton>(true));
            descText.alpha = 0f;
            _textTransitioners.Clear();
            _textTransitioners.Add(new TextTransitioner("Learn more about this mod.", buttons[0]));
            _textTransitioners.Add(new TextTransitioner("Start the tutorial to understand the UI.", buttons[1]));
            _textTransitioners.Add(new TextTransitioner("Open the GitHub page (in your browser).", buttons[2]));
            _textTransitioners.Add(new TextTransitioner("Donate to the mod creator (opens in browser).", buttons[3]));
            _textTransitioners.Add(new TextTransitioner("Create, switch, and remove your config profile(s).", buttons[4]));
            _textTransitioners.Add(new TextTransitioner("Edit the settings for the current profile.", buttons[5]));
            foreach (var transitioner in _textTransitioners)
                transitioner.StateChanged += ButtonSelectionStateChanged;
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