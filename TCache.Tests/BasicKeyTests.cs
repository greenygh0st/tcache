using System;
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
    }
}
