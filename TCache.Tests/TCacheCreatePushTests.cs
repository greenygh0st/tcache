using System;
using Xunit;
using TCache;
using System.Threading.Tasks;
using System.Collections.Generic;
using TCache.Tests.Models;
using TCache.Tests.TestHelpers;

/// <summary>
/// These tests are for the TCache library not API/Service
/// </summary>
namespace TCache.Tests
{
    public class TCacheCreatePushTests
    {
        private string _connectionString { get; set; }

        private string TestQueueName = "test:set:one";

        public TCacheCreatePushTests()
        {
            using (TCacheService cache = new TCacheService())
            {
                bool cleaned = cache.RemoveKey(TestQueueName).Result;
            }
        }

        [Fact, TestPriority(1)]
        public async Task CreateQueue()
        {
            TCacheService cache = new TCacheService();

            await cache.PushToQueue(TestQueueName, new List<Cat> { new Cat { Name = "Ted", Age = 3 } });

            Assert.True(await cache.QueueExists(TestQueueName));
        }

        [Fact, TestPriority(2)]
        public async Task PopTedFromQueue()
        {
            TCacheService cache = new TCacheService();
            Cat cat1 = await cache.PopFromQueue<Cat>(TestQueueName, TCachePopMode.Delete);
            Cat cat2 = await cache.PopFromQueue<Cat>(TestQueueName, TCachePopMode.Delete);

            bool done = cat1.Name == "Ted" && cat2 == null;

            Assert.True(done);
        }

        [Fact]
        public async Task RemoveQueue()
        {
            using (TCacheService cache = new TCacheService())
            {   
                await cache.RemoveKey(TestQueueName);
                bool exists = await cache.QueueExists(TestQueueName);

                Assert.False(exists);
            }
        }
    }
}
