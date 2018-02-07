﻿using System.IO;
using System.Reflection;

namespace Agoda.Analyzers.Helpers
{
    public static class DescriptionContentLoader
    {
        private static string localPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Load analyzer html description from the project
        /// </summary>
        /// <param name="analyzerName">Name of the analyzer.</param>
        /// <returns>Content of the analyzer description.</returns>
        public static string GetAnalyzerDescription(string analyzerName)
        {
            string path = Path.Combine(localPath, $"RuleContent\\{analyzerName}.html");
            var content = File.ReadAllText(path);
            return content;
        }
    }
}
