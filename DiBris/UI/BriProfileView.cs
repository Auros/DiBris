using Zenject;
using System.Linq;
using DiBris.Managers;
using System.Threading.Tasks;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using HMUI;
using IPA.Utilities;

namespace DiBris.UI
{
    [ViewDefinition("DiBris.UI.Views.profile-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\profile-view.bsml")]
    internal class BriProfileView : BSMLAutomaticViewController
    {
        [Inject]
        protected readonly Config _config = null!;

        [Inject]
        protected readonly ProfileManager _profileManager = null!;

        [UIComponent("profile-list")]
        protected readonly CustomListTableData profileList = null!;

        private string _statusText = " ";
        [UIValue("status-text")]
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                NotifyPropertyChanged();
            }
        }

        private string _activeProfile = "![NOT SET]";
        [UIValue("active-profile")]
        public string ActiveProfile
        {
            get => _activeProfile;
            set
            {
                _activeProfile = $"Active Profile - {value}";
                NotifyPropertyChanged();
            }
        }

        protected int? SelectedIndex
        {
            get
            {
                var selectedIndexies = profileList.tableView.GetField<HashSet<int>, TableView>("_selectedCellIdxs");
                if (selectedIndexies.Count == 0) return null;
                return selectedIndexies.First();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            ActiveProfile = _config.Name;
            _ = ReloadProfiles(true);
        }

        private async Task ReloadProfiles(bool updateText = false)
        {
            profileList.data.Clear();
            profileList.tableView.ReloadData();
            IEnumerable<Config> profiles = await _profileManager.AllSubProfiles();
            foreach (var profile in profiles)
            {
                profileList.data.Add(new ConfigCellInfo(profile));
            }
            profileList.tableView.ReloadData();

            if (updateText)
                StatusText = $"Loaded {profileList.data.Count} profiles.";
        }

        [UIAction("save")]
        protected async Task SaveCurrent()
        {
            IEnumerable<Config> profiles = await _profileManager.AllSubProfiles();
            bool overwrite = profiles.Any(u => u.Name == _config.Name);
            if (overwrite)
                StatusText = $"<color=#f5b642>Profile overwritten.</color>";
            else
                StatusText = "<color=#32d62f>Profile saved.</color>";
            _profileManager.Save(_config);
            await ReloadProfiles();
            if (!overwrite)
                profileList.tableView.ScrollToCellWithIdx(profileList.data.Count(), TableViewScroller.ScrollPositionType.End, true);
        }

        [UIAction("load")]
        protected void Load()
        {
            if (SelectedIndex is null)
            {
                StatusText = "<color=#f54242>No Profile Selected.</color>";
                return;
            }
            var config = (profileList.data[SelectedIndex.Value] as ConfigCellInfo)!;
            _config.CopyFrom(config.Config);
            ActiveProfile = config.Config.Name;
            StatusText = "<color=#32d62f>Profile applied!</color>";
        }

        [UIAction("delete")]
        protected async Task Delete()
        {
            if (SelectedIndex is null)
            {
                StatusText = "<color=#f54242>No Profile Selected.</color>";
                return;
            }
            var config = (profileList.data[SelectedIndex.Value] as ConfigCellInfo)!;
            _profileManager.Delete(config.Config);
            StatusText = "<color=#32d62f>Profile deleted!</color>";
            await ReloadProfiles();
        }

        private class ConfigCellInfo : CustomCellInfo
        {
            public Config Config { get; }

            public ConfigCellInfo(Config config) : base(config.Name, "", null!)
            {
                Config = config;
            }
        }
    }
}