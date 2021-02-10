using System;
using Zenject;
using UnityEngine;
using DiBris.Managers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;

namespace DiBris.UI.Settings
{
    internal class General : Parseable, IRefreshable
    {
        public override string Name => nameof(General);

        public override string ContentPath => $"{nameof(DiBris)}.Views.Settings.{nameof(General).ToLower()}.bsml";

        [Inject]
        protected readonly Config _config = null!;

        [Inject]
        protected readonly ProfileManager _profileManager = null!;

        [UIComponent("mirror-list")]
        protected CustomCellListTableData mirrorTable = null!;

        [UIComponent("mirror-root")]
        protected RectTransform mirrorRoot = null!;

        [UIValue("profile-name")]
        protected string ProfileName
        {
            get => _config.Name;
            set => _config.Name = value;
        }

        [UIValue("remove-all-debris")]
        protected bool RemoveAllDebris
        {
            get => _config.RemoveDebris;
            set => _config.RemoveDebris = value;
        }

        public override async void Refresh()
        {
            mirrorTable.data.Clear();
            mirrorRoot.gameObject.SetActive(true);
            foreach (Transform t in mirrorTable.tableView.contentTransform)
            {
                UnityEngine.Object.Destroy(t.gameObject);
            }
            foreach (var mirror in _config.MirrorConfigs)
            {
                mirrorTable.data.Add(new Cell(mirror, true, MirrorChange));
            }
            foreach (var config in await _profileManager.AllSubProfiles())
            {
                if (!_config.MirrorConfigs.Contains(config.Name) && _config.Name != config.Name)
                {
                    mirrorTable.data.Add(new Cell(config.Name, false, MirrorChange));
                }
            }
            mirrorTable.tableView.ReloadData();
            if (mirrorTable.data.Count == 0)
            {
                mirrorRoot.gameObject.SetActive(false);
            }
            base.Refresh();
        }

        public void MirrorChange(string name, bool state)
        {
            if (state)
                _config.MirrorConfigs.Add(name);
            else
                _config.MirrorConfigs.Remove(name);
        }

        public class Cell : INotifyPropertyChanged
        {
            [UIValue("name")]
            public string Name { get; }

            protected bool _enabled;
            [UIValue("enabled")]
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    NotifyPropertyChanged(nameof(Enabled));
                    NotifyPropertyChanged(nameof(Status));
                    NotifyPropertyChanged(nameof(StatusColor));
                    NotifyPropertyChanged(nameof(ToggleString));
                }
            }

            [UIValue("status")]
            public string Status => Enabled ? "✅" : "❌";

            [UIValue("status-color")]
            public string StatusColor => Enabled ? "lime" : "red";

            [UIValue("toggle-string")]
            public string ToggleString => Enabled ? "-" : "+";

            private Action<string, bool> Changed;
            public event PropertyChangedEventHandler? PropertyChanged;

            public Cell(string name, bool initialState, Action<string, bool> onStateChange)
            {
                Name = name;
                Changed = onStateChange;
                _enabled = initialState;
            }

            [UIAction("change-state")]
            protected void ChangedState()
            {
                Enabled = !Enabled;
                Changed?.Invoke(Name, Enabled);
            }

            protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                catch { }
            }
        }
    }
}