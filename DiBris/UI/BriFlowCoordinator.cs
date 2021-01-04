using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace DiBris.UI
{
    internal class BriFlowCoordinator : FlowCoordinator
    {
        private BriMainView _briMainView = null!;

        private MainFlowCoordinator _mainFlowCoordinator = null!;

        [Inject]
        public void Construct(BriMainView briMainView, MainFlowCoordinator mainFlowCoordinator)
        {
            _briMainView = briMainView;
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
        }

        protected override void BackButtonWasPressed(ViewController _)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}