﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace TCache
{
    public class TCacheService : IDisposable
    {
        private bool disposedValue;
        private ConnectionMultiplexer _redis;
        private string _redisUri;

        /// <summary>
        /// TCacheService constructor, supply redis uri
        /// </summary>
        /// <param name="redisUri">The Redis server you want to connect to</param>
        public TCacheService(string redisUri)
        {
            _redisUri = redisUri;
            _redis = ConnectionMultiplexer.Connect(redisUri);
        }

        /// <summary>
        /// TCacheService constructor, assumes localhost:6379 for Redis URI
        /// </summary>
        public TCacheService()
        {
            _redisUri = "localhost:6379";
            _redis = ConnectionMultiplexer.Connect(_redisUri);
        }

        /// <summary>
        /// Retrieve the string value from a given key
        /// </summary>
        /// <param name="key">The key you want to fetch</param>
        /// <returns></returns>
        public async Task<string> GetValueFromKey(string key)
        {
            IDatabase db = _redis.GetDatabase();

            var value = await db.StringGetAsync(key);

            if (value.IsNull)
            {
                return null;
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Search for keys that meet a particular pattern and return their key names.
        /// </summary>
        /// <param name="searchPattern">Search term, use wildcard characters '*' before and after your search term to search non-specifically.</param>
        /// <returns>List of found keys</returns>
        public async Task<List<string>> SearchKeys(string searchPattern)
        {
            using ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(_redisUri);
            var endpoint = connection.GetEndPoints()[0];
            var server = connection.GetServer(endpoint);
            //var server = _redis.GetServer(_redisUri);


            List<string> keys = new List<string>();

            await foreach (var item in server.KeysAsync(pattern: searchPattern))
                keys.Add(item.ToString());

            return keys;
        }

        /// <summary>
        /// Return the keys and values of the searched objects
        /// </summary>
        /// <param name="searchPattern">Search term, use wildcard characters '*' before and after your search term to search non-specifically.</param>
        /// <returns>A dictionary of key value pairs.</returns>
        public async Task<Dictionary<string, string>> SearchKeyValues(string searchPattern)
        {
            IDatabase db = _redis.GetDatabase();
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            List<Task> fetchTasks = new List<Task>();

            foreach (var item in await SearchKeys(searchPattern))
            {
                fetchTasks.Add(Task.Run(async () => {
                    var result = await GetValueFromKey(item);
                    keyValuePairs.Add(item, result);
                }));
            }

            Task.WaitAll(fetchTasks.ToArray());

            return keyValuePairs;
        }

        /// <summary>
        /// Return the keys and typed values of the searched objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchPattern">Search term, use wildcard characters '*' before and after your search term to search non-specifically.</param>
        /// <returns>A dictionary of keys and typed value pairs.</returns>
        public async Task<Dictionary<string, T>> SearchKeyValues<T>(string searchPattern)
        {
            IDatabase db = _redis.GetDatabase();
            Dictionary<string, T> keyValuePairs = new Dictionary<string, T>();
            List<Task> fetchTasks = new List<Task>();

            foreach (var item in await SearchKeys(searchPattern))
            {
                fetchTasks.Add(Task.Run(async () => {
                    var result = await GetValueFromKey<T>(item);
                    keyValuePairs.Add(item, result);
                }));
            }

            Task.WaitAll(fetchTasks.ToArray());

            return keyValuePairs;
        }

        /// <summary>
        /// Retrieve a given object from a given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key you want to retrieve</param>
        /// <returns></returns>
        public async Task<T> GetValueFromKey<T>(string key)
        {
            IDatabase db = _redis.GetDatabase();

            var value = await db.StringGetAsync(key);

            if (value.IsNull)
            {
                return default(T);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
        }

        /// <summary>
        /// Store a given a given object under a particular key
        /// </summary>
        /// <param name="key">The key to store the object under</param>
        /// <param name="value">The object to be stored</param>
        /// <returns></returns>
        public async Task<bool> SetObjectAsKeyValue(string key, object value)
        {
            IDatabase db = _redis.GetDatabase();

            string valueToStore = (value is string) ? value.ToString() : JsonConvert.SerializeObject(value);

            return await db.StringSetAsync(new RedisKey(key), new RedisValue(valueToStore));
        }

        /// <summary>
        /// Store a given a given object under a particular key
        /// </summary>
        /// <param name="key">The key to store the object under</param>
        /// <param name="value">The object to be stored</param>
        /// <param name="expires">Timespan which represents the time until expiration/param>
        /// <returns></returns>
        public async Task<bool> SetObjectAsKeyValue(string key, object value, TimeSpan expires)
        {
            IDatabase db = _redis.GetDatabase();

            string valueToStore = (value is string) ? value.ToString() : JsonConvert.SerializeObject(value);

            return await db.StringSetAsync(new RedisKey(key), new RedisValue(valueToStore), expires);
        }

        /// <summary>
        /// Remove the specified key
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <returns></returns>
        public async Task<bool> RemoveKey(string key)
        {
            IDatabase db = _redis.GetDatabase();

            if (await KeyExists(key))
            {
                await db.KeyDeleteAsync(key);
            }

            return true;
        }

        /// <summary>
        /// Check to see if a key exists
        /// </summary>
        /// <param name="key"><see cref="string"/> value for the key you want to check.</param>
        /// <returns></returns>
        public async Task<bool> KeyExists(string key)
        {
            IDatabase db = _redis.GetDatabase();

            return await db.KeyExistsAsync(new RedisKey(key));
        }

        /// <summary>
        /// Push data to a queue. If the queue does not exist it will be created
        /// </summary>
        /// <param name="queueName">The name of the queue name you want to push to</param>
        /// <param name="values">The set of values you want to push the queue.</param>
        /// <returns></returns>
        public async Task<bool> PushToQueue<T>(string queueName, List<T> values)
        {
            IDatabase db = _redis.GetDatabase();

            // we need to send an empty array to create the queue
            List<RedisValue> valuesToPush = new List<RedisValue>();

            foreach (var item in values)
            {
                valuesToPush.Add((item is string) ? item.ToString() : JsonConvert.SerializeObject(item));
            }

            await db.ListRightPushAsync(new RedisKey($"queue:{queueName}"), valuesToPush.ToArray());

            return true;
        }

        public async Task<string> PopFromQueue(string queueName, TCachePopMode popMode = TCachePopMode.Delete)
        {
            IDatabase db = _redis.GetDatabase();
            string queueFullName = $"queue:{queueName}";

            RedisValue value = (popMode == TCachePopMode.Delete) ? await db.ListLeftPopAsync(queueFullName) : await db.ListGetByIndexAsync(queueFullName, 0); // 

            if (!value.IsNullOrEmpty)
            {
                return value.ToString();
            }
            else
            {
                return null;
            }
        }

        public async Task<T> PopFromQueue<T>(string queueName, TCachePopMode popMode = TCachePopMode.Delete)
        {
            IDatabase db = _redis.GetDatabase();
            string queueFullName = $"queue:{queueName}";

            RedisValue value = (popMode == TCachePopMode.Delete) ? await db.ListLeftPopAsync(queueFullName) : await db.ListGetByIndexAsync(queueFullName, 0); // 

            if (!value.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<T>(value);
            } else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Check to see if a queue exists
        /// </summary>
        /// <param name="key"><see cref="string"/> value for the key you want to check</param>
        /// <returns></returns>
        public async Task<bool> QueueExists(string key)
        {
            return await KeyExists($"queue:{key}");
        }

        /// <summary>
        /// Remove a queue from Redis
        /// </summary>
        /// <param name="key"><see cref="string"/> value for the key you want to check</param>
        /// <returns></returns>
        public async Task<bool> RemoveQueue(string key)
        {
            return await RemoveKey($"queue:{key}");
        }

        #region Disposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;

                //_redis.Close();
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RedisService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
