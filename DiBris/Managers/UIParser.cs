using Zenject;
using DiBris.UI;
using System.IO;
using IPA.Loader;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using System.Collections.Generic;

namespace DiBris.Managers
{
    internal class UIParser
    {
        private readonly DiContainer _container;
        private readonly List<Parseable> _parseables;
        private readonly PluginMetadata _pluginMetadata;

        public UIParser(DiContainer container, [Inject(Id = nameof(DiBris))] PluginMetadata pluginMetadata)
        {
            _container = container;
            _pluginMetadata = pluginMetadata;
            _parseables = new List< Parseable>();
        }

        public async Task Parse(Parseable parseable)
        {
            if (_parseables.Contains(parseable))
                return;

            // Load BSML Content (Asynchronously)
            Stream stream = _pluginMetadata.Assembly.GetManifestResourceStream(parseable.ContentPath);
            StreamReader sr = new StreamReader(stream);
            string content = await sr.ReadToEndAsync();
            sr.Dispose();
            stream.Dispose();

            _parseables.Add(parseable);
            _container.Inject(parseable);
            BSMLParser.instance.Parse(content, parseable.root.gameObject, parseable);
        }
    }
}