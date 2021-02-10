using System;
using Zenject;
using DiBris.UI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

namespace DiBris.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly BriFlowCoordinator _briFlowCoordinator;
        private readonly MainFlowCoordinator _mainFlowCoordinator;

        public MenuButtonManager(BriFlowCoordinator briFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
        {
            _briFlowCoordinator = briFlowCoordinator;
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuButton = new MenuButton(nameof(DiBris), ShowFlow);
        }

        private void ShowFlow() => _mainFlowCoordinator.PresentFlowCoordinator(_briFlowCoordinator);
        public void Initialize() => MenuButtons.instance.RegisterButton(_menuButton);

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
                MenuButtons.instance.UnregisterButton(_menuButton);
        }
    }
}