using UnityEngine;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI
{
    internal abstract class Parseable
    {
        [UIValue("name")]
        public abstract string Name { get; }
        public abstract string ContentPath { get; }

        [UIComponent("root")]
        public RectTransform root = null!;
    }
}