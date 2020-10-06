﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCache.Tests.Models;
using TCache.Tests.TestHelpers;
using Xunit;

namespace TCache.Tests
{
    [Collection("Tests")]
    public class TCacheGetTests
    {
        private string TestQueueName = "test:set:two";
        private string TestQueueNameEmpty = "test:set:empty";
        private string TestRemoveQueue = "test:set:remove";

        public TCacheGetTests()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                bool cleaned = cache.RemoveKey(TestQueueName).Result;
            }
        }


        [Fact, TestPriority(1)]
        public async Task GetQueue()
        {
            TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri);
            await cache.PushToQueue(TestQueueName, new List<Cat> { new Cat { Name = "NotDead", Age = 3 } });
            Cat cat1 = await cache.PopFromQueue<Cat>(TestQueueName, TCachePopMode.Get);
            Cat cat2 = await cache.PopFromQueue<Cat>(TestQueueName, TCachePopMode.Get);
            bool done = cat1.Name == "NotDead" && cat2.Name == "NotDead";

            // make sure the queue is empty
            Cat cat3 = await cache.PopFromQueue<Cat>(TestQueueName, TCachePopMode.Delete);
            Cat cat4 = await cache.PopFromQueue<Cat>(TestQueueName, TCachePopMode.Delete);

            if (cat3 != null)
                Console.WriteLine(cat3.Name);

            if (cat4 != null)
                Console.WriteLine(cat4.Name);

            done = done && cat3 != null && cat4 == null;

            // remove the queue
            await cache.RemoveQueue(TestQueueName);

            // result
            Assert.True(done);
        }

        [Fact]
        public async Task GetEmptyQueue()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                Cat noCat = await cache.PopFromQueue<Cat>(TestQueueNameEmpty);
                Assert.Null(noCat);
            }
        }

        [Fact]
        public async Task TestRemoveExistingQueue()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.PushToQueue(TestRemoveQueue, new List<Cat> { new Cat { Name = "Kel", Age = 12 } });
                await cache.RemoveQueue(TestRemoveQueue);
                bool result = await cache.QueueExists(TestRemoveQueue);

                Assert.False(result);
            }
        }
    }
}
