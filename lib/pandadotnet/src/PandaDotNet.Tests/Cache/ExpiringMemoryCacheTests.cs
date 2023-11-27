using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.Cache.Abstraction;
using PandaDotNet.Cache.ExpiringCache;
using PandaDotNet.Time;

namespace PandaDotNet.Tests.Cache
{
    [TestClass]
    public class ExpiringMemoryCacheTests
    {
        private readonly ICache<int, int> _cache;
        private readonly FixedClock _clock;

        private int dataSourceCalled = 0;

        public ExpiringMemoryCacheTests()
        {
            _clock = new FixedClock();
            _cache = new ExpiringMemoryCache<int, int>(TimeSpan.FromMinutes(5), _clock);
        }

        private int DataSource(int key)
        {
            dataSourceCalled++;
            return GenerateRecordForKey(key);
        }

        private static int GenerateRecordForKey(int key)
        {
            return key * 100;
        }

        [TestMethod]
        public void RecordsExpireCorrectly()
        {
            int testRecord = _cache.GetObjectForKey(10, DataSource);
            Assert.AreEqual(GenerateRecordForKey(10), testRecord,
                "Generated record is not as expected");
            Assert.IsTrue(_cache.IsCached(10),
                "Object should've been cached, but isn't");
            Assert.AreEqual(1, dataSourceCalled,
                "Data Source wasn't called one time only");

            _clock.AdvanceTimeBy(6.Minutes());
            Assert.IsFalse(_cache.IsCached(10),
                "Cached object should have expired, but isn't");
            
            testRecord = _cache.GetObjectForKey(10, DataSource);
            Assert.AreEqual(2, dataSourceCalled,
                "Data Source wasn't called 2 times only");
            
            _clock.AdvanceTimeBy(2.Minutes());
            Assert.IsTrue(
                _cache.IsCached(10),
                "Cached object was invalidated before it should have been");

        }
    }
}