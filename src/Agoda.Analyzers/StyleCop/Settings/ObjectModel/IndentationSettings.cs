using Newtonsoft.Json;

namespace Agoda.Analyzers.StyleCop.Settings.ObjectModel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class IndentationSettings
    {
        /// <summary>
        /// This is the backing field for the <see cref="IndentationSize"/> property.
        /// </summary>
        [JsonProperty("indentationSize", DefaultValueHandling = DefaultValueHandling.Include)] private int indentationSize;

        /// <summary>
        /// This is the backing field for the <see cref="TabSize"/> property.
        /// </summary>
        [JsonProperty("tabSize", DefaultValueHandling = DefaultValueHandling.Include)] private int tabSize;

        /// <summary>
        /// This is the backing field for the <see cref="UseTabs"/> property.
        /// </summary>
        [JsonProperty("useTabs", DefaultValueHandling = DefaultValueHandling.Include)] private bool useTabs;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentationSettings"/> class during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected internal IndentationSettings()
        {
            indentationSize = 4;
            tabSize = 4;
            useTabs = false;
        }

        public int IndentationSize =>
            indentationSize;

        public int TabSize =>
            tabSize;

        public bool UseTabs =>
            useTabs;
    }
}