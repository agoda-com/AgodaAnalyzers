using Newtonsoft.Json;

namespace Agoda.Analyzers.StyleCop.Settings.ObjectModel
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class SettingsFile
    {
        /// <summary>
        /// This is the backing field for the <see cref="Settings"/> property.
        /// </summary>
        [JsonProperty("settings", DefaultValueHandling = DefaultValueHandling.Ignore)] private StyleCopSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFile"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SettingsFile()
        {
            settings = new StyleCopSettings();
        }

        public StyleCopSettings Settings
        {
            get { return settings; }
        }
    }
}