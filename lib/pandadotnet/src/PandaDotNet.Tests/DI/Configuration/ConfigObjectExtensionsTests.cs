using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.DI.Configuration;

namespace PandaDotNet.Tests.DI.Configuration
{
    [TestClass]
    public class ConfigObjectExtensionsTests
    {
        class MyCoolConfigClass
        {
            public string TestString { get; set; }
            public int TestNumber { get; set; }
        }
        
        [TestMethod]
        public void CanResolveObjectFromConfig()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"MyCoolConfigSection:TestString", "This is a test"},
                    {"MyCoolConfigSection:TestNumber", "42"}
                })
                .Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddConfigObject<MyCoolConfigClass>("MyCoolConfigSection")
                .BuildServiceProvider();

            var cfgObject = serviceProvider.GetRequiredService<MyCoolConfigClass>();
            Assert.IsNotNull(cfgObject);
            
            Assert.AreEqual("This is a test", cfgObject.TestString);
            Assert.AreEqual(42, cfgObject.TestNumber);
        }
    }
}