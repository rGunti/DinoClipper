using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.DI.Configuration;

namespace PandaDotNet.Tests.DI.Configuration
{
    [TestClass]
    public class ConnectionStringExtensionsTests
    {
        private IConfiguration ProduceConfigurationWith(string connectionString)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:Test", connectionString }
                })
                .Build();
        }
        
        [TestMethod]
        [DataRow("file:///tmp/file/cool", "file", "/tmp/file/cool")]
        [DataRow("void://", "void", "")]
        [DataRow("internet://http://example.com", "internet", "http://example.com")]
        [DataRow("missing-scheme", "", "missing-scheme")]
        public void SchemeParsing(
            string inputUri,
            string expectedScheme,
            string expectedConnectionString)
        {
            IConfiguration config = ProduceConfigurationWith(inputUri);

            string connectionString = config.GetConnectionString("Test", false);

            Assert.AreEqual(expectedScheme, connectionString.GetScheme());
            Assert.AreEqual(expectedConnectionString, connectionString.StripScheme());
        }
    }
}