// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Agoda.Analyzers.StyleCop.Settings.ObjectModel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StyleCopSettings
    {
        /// <summary>
        /// This is the backing field for the <see cref="Indentation"/> property.
        /// </summary>
        [JsonProperty("indentation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IndentationSettings indentation;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StyleCopSettings"/> class during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        public StyleCopSettings()
        {
            this.indentation = new IndentationSettings();
            
        }

        public IndentationSettings Indentation =>
            this.indentation;
        
    }
}
