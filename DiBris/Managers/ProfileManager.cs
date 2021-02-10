using System;
using System.IO;
using System.Linq;
using IPA.Utilities;
using SiraUtil.Tools;
using Newtonsoft.Json;
using IPA.Config.Data;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using IPA.Config.Stores.Converters;

namespace DiBris.Managers
{
    internal class ProfileManager
    {
        private bool _loaded;
        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private readonly List<(FileInfo, Config)> _loadedConfigs;
        private readonly DirectoryInfo _profileDirectory;

        public ProfileManager(Config config, SiraLog siraLog)
        {
            _config = config;
            _siraLog = siraLog;
            _loadedConfigs = new List<(FileInfo, Config)>();
            _profileDirectory = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, "Di", "Bris", "Profiles"));
        }

        public async Task<IEnumerable<Config>> GetMirrorConfigs()
        {
            List<Config> theGoodOnes = new List<Config>();
            var profiles = await AllSubProfiles();
            foreach (var profile in profiles)
            {
                if (_config.MirrorConfigs.Contains(profile.Name))
                    theGoodOnes.Add(profile);
            }
            return theGoodOnes;
        }

        public async Task<IEnumerable<Config>> AllSubProfiles()
        {
            if (!_loaded)
            {
                _profileDirectory.Create();

                foreach (var file in _profileDirectory.EnumerateFiles().Where(f => f.Extension == ".json"))
                {
                    try
                    {
                        FileStream fs = file.OpenRead();
                        StreamReader sr = new StreamReader(fs);
                        JsonTextReader jtr = new JsonTextReader(sr);
                        JToken token = await JToken.ReadFromAsync(jtr);
                        Config config = CustomObjectConverter<Config>.Deserialize(VisitToValue(token), null);
                        jtr.Close();
                        sr.Close();
                        fs.Close();
                        _loadedConfigs.Add((file, config));
                    }
                    catch
                    {
                        _siraLog.Error($"Error loading profile at {file.Name}");
                    }
                }

                _loaded = true;
            }
            return _loadedConfigs.Select(lf => lf.Item2);
        }

        public void Save(Config config)
        {
            (FileInfo, Config) fileConf = _loadedConfigs.FirstOrDefault(lf => lf.Item2.Name == config.Name);
            try
            {
                Value val = CustomObjectConverter<Config>.Serialize(config, null);
                FileInfo file = new FileInfo(Path.Combine(_profileDirectory.FullName, $"{config.Name}.json"));
                File.WriteAllText(file.FullName, val.ToString());
                if (fileConf == default)
                    _loadedConfigs.Add((file, CustomObjectConverter<Config>.Deserialize(val, null)));
                else
                {
                    _loadedConfigs.Remove(fileConf);
                    _loadedConfigs.Add((file, CustomObjectConverter<Config>.Deserialize(val, null)));
                }
            }
            catch (Exception e)
            {
                _siraLog.Error($"Could not save config. {e.Message}");
            }
        }

        public void Delete(Config config)
        {
            (FileInfo, Config) fileConf = _loadedConfigs.FirstOrDefault(lf => lf.Item2.Name == config.Name);
            if (fileConf == default)
                return;

            _loadedConfigs.Remove(fileConf);
            if (fileConf.Item1.Exists)
            {
                fileConf.Item1.Delete();
            }
        }

        // i literally just stole this from danike.
        private Value VisitToValue(JToken tok)
        {
            if (tok == null) return Value.Null();

            switch (tok.Type)
            {
                case JTokenType.Null:
                    return Value.Null();
                case JTokenType.Boolean:
                    return Value.Bool(((tok as JValue)!.Value as bool?) ?? false);
                case JTokenType.String:
                    var val = (tok as JValue)!.Value;
                    if (val is string s) return Value.Text(s);
                    else if (val is char c) return Value.Text("" + c);
                    else return Value.Text(string.Empty);
                case JTokenType.Integer:
                    val = (tok as JValue)!.Value;
                    if (val is long l) return Value.Integer(l);
                    else if (val is ulong u) return Value.Integer((long)u);
                    else return Value.Integer(0);
                case JTokenType.Float:
                    val = (tok as JValue)!.Value;
                    if (val is decimal dec) return Value.Float(dec);
                    else if (val is double dou) return Value.Float((decimal)dou);
                    else if (val is float flo) return Value.Float((decimal)flo);
                    else return Value.Float(0);
                case JTokenType.Array:
                    return Value.From((tok as JArray).Select(VisitToValue));
                case JTokenType.Object:
                    return Value.From((tok as IEnumerable<KeyValuePair<string, JToken>>)
                        .Select(kvp => new KeyValuePair<string, Value>(kvp.Key, VisitToValue(kvp.Value))));
                default:
                    throw new ArgumentException($"Unknown {nameof(JTokenType)} in parameter");
            }
        }
    }
}