using Newtonsoft.Json;

namespace Agoda.Analyzers.StyleCop.Settings.ObjectModel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StyleCopSettings
    {
        /// <summary>
        /// This is the backing field for the <see cref="Indentation"/> property.
        /// </summary>
        [JsonProperty("indentation", DefaultValueHandling = DefaultValueHandling.Ignore)] private IndentationSettings indentation;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleCopSettings"/> class during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        public StyleCopSettings()
        {
            indentation = new IndentationSettings();
        }

        public IndentationSettings Indentation =>
            indentation;
    }
}