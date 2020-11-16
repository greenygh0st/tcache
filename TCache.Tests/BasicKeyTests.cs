using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCache.Tests.Models;
using Xunit;

namespace TCache.Tests
{
    [Collection("Tests")]
    public class BasicKeyTests
    {
        [Fact]
        public async Task RemoveKeyNoKey()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                bool result = await cache.RemoveKey("blah blah blah");
                Assert.True(result);
            }
        }

        [Fact]
        public async Task RemoveKeyWithKey()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                // set
                await cache.SetObjectAsKeyValue("test1", "blah blah blah");

                // remove
                await cache.RemoveKey("test1");

                // check
                string result = await cache.GetValueFromKey("test1");

                // asert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task SetKey()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("test2", "hi!");

                string value = await cache.GetValueFromKey("test2");

                await cache.RemoveKey("test2");

                Assert.Equal("hi!", value);
            }
        }

        [Fact]
        public async Task SetKeyExpiring()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("test3", new Cat { }, TimeSpan.FromSeconds(5));
                bool initialCheck = await cache.KeyExists("test3");
                await Task.Delay(TimeSpan.FromSeconds(6));
                bool finalCheck = await cache.KeyExists("test3");

                Assert.True(initialCheck && !finalCheck);
            }
        }

        [Fact]
        public async Task GetEmptyValue()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                string value = await cache.GetValueFromKey("test4");

                Assert.Null(value);
            }
        }

        [Fact]
        public async Task GetEmptyTypesValue()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                Cat value = await cache.GetValueFromKey<Cat>("test4");

                Assert.Null(value);
            }
        }

        [Fact]
        public async Task TypedValueFetch()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("test5", new Cat { Name = "Billy", Age = 90 });

                var result = await cache.GetValueFromKey<Cat>("test5");
                await cache.RemoveKey("test5");

                Assert.True(result is Cat && result.Name == "Billy" && result.Age == 90);
            }
        }

        [Fact]
        public async Task KeyExistsTest()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("test6", "test");

                bool result = await cache.KeyExists("test6");
                await cache.RemoveKey("test6");

                Assert.True(result);
            }
        }

        [Fact]
        public async Task SearchKeys()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("user:test1:1", "test");
                await cache.SetObjectAsKeyValue("user:test1:2", "test");
                await cache.SetObjectAsKeyValue("user:test1:3", "test");
                await cache.SetObjectAsKeyValue("user:test1:4", "test");

                List<string> results = await cache.SearchKeys("user:test1*");

                Assert.Equal(4, results.Count);

                foreach (var item in results)
                {
                    await cache.RemoveKey(item);
                }
            }
        }

        [Fact]
        public async Task SearchKeyValues()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("user:test2:5", "test1");
                await cache.SetObjectAsKeyValue("user:test2:6", "test2");
                await cache.SetObjectAsKeyValue("user:test2:7", "test3");
                await cache.SetObjectAsKeyValue("user:test2:8", "test4");

                var results = await cache.SearchKeyValues("user:test2*");

                bool resultCheck = results.ContainsValue("test1") && results.ContainsValue("test2") && results.ContainsValue("test3") && results.ContainsValue("test4");

                Assert.Equal(4, results.Count);
                Assert.True(resultCheck);

                foreach (var item in results.Keys)
                {
                    await cache.RemoveKey(item);
                }
            }
        }

        [Fact]
        public async Task SearchKeyValuesTyped()
        {
            using (TCacheService cache = new TCacheService(TestConfiguration.EnvRedisUri))
            {
                await cache.SetObjectAsKeyValue("user:test3:9", new Cat { Name = "Ted" });
                await cache.SetObjectAsKeyValue("user:test3:10", new Cat { Name = "Fred" });
                await cache.SetObjectAsKeyValue("user:test3:11", new Cat { Name = "Ned" });
                await cache.SetObjectAsKeyValue("user:test3:12", new Cat { Name = "Bed" });

                var results = await cache.SearchKeyValues<Cat>("user:test3*");

                List<string> resultValues = results.Values.Select(x => x.Name).ToList();

                bool resultCheck = resultValues.Contains("Ted") && resultValues.Contains("Fred") && resultValues.Contains("Ned") && resultValues.Contains("Bed");

                Assert.Equal(4, results.Count);
                Assert.True(resultCheck);

                foreach (var item in results.Keys)
                {
                    await cache.RemoveKey(item);
                }
            }
        }
    }
}
