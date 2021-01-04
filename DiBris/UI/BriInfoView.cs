using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.Views.info-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\info-view.bsml")]
    internal class BriInfoView : BSMLAutomaticViewController
    {
        
    }
}