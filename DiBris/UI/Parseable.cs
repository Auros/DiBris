using UnityEngine;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;

namespace DiBris.UI
{
    internal abstract class Parseable : IRefreshable
    {
        [UIValue("name")]
        public abstract string Name { get; }
        public abstract string ContentPath { get; }

        [UIParams]
        protected BSMLParserParams parserParams = null!;

        [UIComponent("root")]
        public RectTransform root = null!;

        public virtual void Refresh()
        {
            parserParams?.EmitEvent("update");
        }
    }
}