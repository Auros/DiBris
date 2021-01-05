using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace DiBris.UI
{
    internal class BriFlowCoordinator : FlowCoordinator
    {
        private BriMainView _briMainView = null!;
        private BriInfoView _briInfoView = null!;
        private BriProfileView _briProfileView = null!;
        private BriSettingsView _briSettingsView = null!;

        private MainFlowCoordinator _mainFlowCoordinator = null!;

        [Inject]
        public void Construct(BriMainView briMainView, BriInfoView briInfoView, BriProfileView briProfileView, BriSettingsView briSettingsView, MainFlowCoordinator mainFlowCoordinator)
        {
            _briMainView = briMainView;
            _briInfoView = briInfoView;
            _briProfileView = briProfileView;
            _briSettingsView = briSettingsView;
            _mainFlowCoordinator = mainFlowCoordinator;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool _)
        {
            if (firstActivation)
            {
                showBackButton = true;
                SetTitle(nameof(DiBris));
            }
            if (addedToHierarchy) ProvideInitialViewControllers(_briMainView);
            _briMainView.EventNavigated += NavigationReceived;
        }

        private void NavigationReceived(NavigationEvent navEvent)
        {
            switch (navEvent)
            {
                case NavigationEvent.Info:
                    SetLeftScreenViewController(_briInfoView, ViewController.AnimationType.In);
                    SetRightScreenViewController(null, ViewController.AnimationType.Out);
                    break;
                case NavigationEvent.Profile:
                    SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                    SetRightScreenViewController(_briProfileView, ViewController.AnimationType.In);
                    break;
                case NavigationEvent.Settings:
                    SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                    SetRightScreenViewController(_briSettingsView, ViewController.AnimationType.In);
                    break;
                default:
                    SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                    SetRightScreenViewController(null, ViewController.AnimationType.Out);
                    break;
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            _briMainView.EventNavigated -= NavigationReceived;
        }

        protected override void BackButtonWasPressed(ViewController _)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }

        public enum NavigationEvent
        {
            Info,
            Profile,
            Settings,
            Unknown
        }
    }
}