using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration;

namespace AkkaPocUtils
{
    /// <summary>
    // from webcrawler example
    /// </summary>
    public static class HoconLoader
    {
        public static Akka.Configuration.Config ParseConfig(string hoconPath)
        {
            return ConfigurationFactory.ParseString(File.ReadAllText(hoconPath));
        }
        public static Akka.Configuration.Config ParseDefaultConfig()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var exeName = Assembly.GetEntryAssembly().GetName().Name;
            var hoconPath = Path.Combine(exeDir, $"{exeName}.hocon");
            if (!File.Exists(hoconPath))
            {
                hoconPath = Path.Combine(exeDir, $"{exeName}.conf");
            }
            return ParseConfig(hoconPath);
        }
    }
}
