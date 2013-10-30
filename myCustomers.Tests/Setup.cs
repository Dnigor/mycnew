using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace myCustomers.Tests
{
    [TestClass]
    public static class Setup
    {
        static void ConfigureNLog()
        {
            var assembly = typeof(Setup).Assembly;
            var name = assembly.GetManifestResourceNames().First(n => n.EndsWith("NLog.config"));
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(new XmlTextReader(assembly.GetManifestResourceStream(name)), "NLog.config");
        }

        [AssemblyInitialize]
        public static void OnStartUp(TestContext context)
        {
            ConfigureNLog();
        }

        [AssemblyCleanup]
        public static void OnShutdown()
        {
        }
    }
}
