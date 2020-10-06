using System;
using System.Collections.Generic;
using System.Text;

namespace TCache.Tests
{
    class TestConfiguration
    {
        public static readonly string ENVRedisUri = Environment.GetEnvironmentVariable("TEST_REDIS_URI");
    }
}
