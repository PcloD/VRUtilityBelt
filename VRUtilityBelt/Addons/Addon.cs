﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRUtilityBelt.Addons.Overlays;
using VRUtilityBelt.Addons.Plugins;
using VRUtilityBelt.Addons.Thermes;

namespace VRUtilityBelt.Addons
{
    public class Addon
    {
        FileSystemWatcher _folderWatcher;

        #region Datums

        [JsonProperty("key")]
        public String Key { get; set; }

        [JsonProperty("name")]
        public String Name { get; set; }

        [JsonProperty("description")]
        public String Description { get; set; }

        [JsonProperty("overlays")]
        public List<string> OverlayKeys { get; set; }

        [JsonIgnore]
        public List<IOverlay> Overlays { get; set; }

        [JsonProperty("themes")]
        public List<string> ThemeKeys { get; set; }

        [JsonIgnore]
        public List<ITheme> Themes { get; set; }

        [JsonProperty("plugins")]
        public List<string> PluginKeys { get; set; }

        [JsonIgnore]
        public List<IPlugin> Plugins { get; set; }

        private String ManifestPath { get { return _path + "\\manifest.json"; } }

        public string DerivedKey
        {
            get
            {
                return _keyPrefix + "_" + Key;
            }
        }

        public Dictionary<string, object> Interops { get; set; } = new Dictionary<string, object>();

        string _path;
        string _keyPrefix = "builtin";

#endregion

        public static Addon Parse(string folder, string keyPrefix = "builtin")
        {
            Addon newAddon = new Addon();
            newAddon._keyPrefix = keyPrefix;
            newAddon._path = folder;
            newAddon.ProcessManifest();

            Console.WriteLine("[ADDON] Found Addon: " + newAddon.Key + " - " + newAddon.Name);

            newAddon.SetupOverlays();
            newAddon.SetupThemes();
            newAddon.SetupPlugins();

            newAddon.SetupFileWatchers();

            return newAddon;
        }

        void ProcessManifest()
        {
            if(!File.Exists(ManifestPath))
            {
                throw new FileNotFoundException("Cannot locate manifest.json in " + _path);
            }

            try
            {
                JsonConvert.PopulateObject(File.ReadAllText(ManifestPath), this);
            } catch(Exception e)
            {
                Console.WriteLine("[JSON] Failed to parse Addon Manifest at " + ManifestPath + ": " + e.Message);
            }
        }

        void SetupFileWatchers()
        {
            _folderWatcher = new FileSystemWatcher(_path);
            _folderWatcher.IncludeSubdirectories = true;

            _folderWatcher.Changed += _folderWatcher_Updated;
            _folderWatcher.Deleted += _folderWatcher_Updated;
            _folderWatcher.Renamed += _folderWatcher_Updated;
        }

        void SetupOverlays()
        {
            Overlays = new List<IOverlay>();

            foreach(string key in OverlayKeys)
            {
                BasicOverlay newOverlay = new BasicOverlay(this);
                newOverlay.Setup(_path + "\\overlays\\" + key);

                Overlays.Add(newOverlay);
            }
        }

        void SetupThemes()
        {
            // Coming sooner than plugins.
        }

        void SetupPlugins()
        {
            // Coming... eventually.
        }

        private void _folderWatcher_Updated(object sender, FileSystemEventArgs e)
        {
            if(Directory.Exists(_path))
                Refresh();
        }

        public void Refresh()
        {
            // TODO: Make it refresh
        }

        void Dispose()
        {
            if(_folderWatcher != null)
                _folderWatcher.Dispose();
        }
    }
}
