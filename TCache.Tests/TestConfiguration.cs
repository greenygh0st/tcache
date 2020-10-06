using System;
using System.Collections.Generic;
using System.Text;

namespace TCache.Tests
{
    class TestConfiguration
    {
        public static readonly string EnvRedisUri = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_REDIS_URI")) ? "localhost:6379" : Environment.GetEnvironmentVariable("TEST_REDIS_URI");
    }
}
