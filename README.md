# TCache
![Master](https://github.com/greenygh0st/tcache/workflows/Master/badge.svg?branch=master) [![codecov](https://codecov.io/gh/greenygh0st/tcache/branch/master/graph/badge.svg)](https://codecov.io/gh/greenygh0st/tcache) [![nuget](https://img.shields.io/nuget/v/TCache.Redis)](https://www.nuget.org/packages/TCache.Redis/) [![nugetname](https://img.shields.io/badge/nuget%20package-TCache.Redis-brightgreen)](https://www.nuget.org/packages/TCache.Redis/)

TCache is a Redis wrapper designed to simplify some basic operations with Redis in C# and provide easy to use typable response methods.

Constuctors
```
using TCache;

// provide a path to redis
public TCacheService(string redisUri)

// assumes localhost:6379 for Redis URI
public TCacheService()
```

Methods
```
// set a given object as the value of a key
public async Task<bool> SetObjectAsKeyValue(string key, object value)

// set a given object as the value of a key with an expiration
public async Task<bool> SetObjectAsKeyValue(string key, object value, TimeSpan? expires)

// fetch the string value for a given key
public async Task<string> GetValueFromKey(string key)

// get the types value for a given key
public async Task<T> GetValueFromKey<T>(string key)

// check to see if a key exists, note the fetch functions will return null for empty keys making this useful for just checking
public async Task<bool> KeyExists(string key)

// remove the specified key from Redis
public async Task<bool> RemoveKey(string key)

// push to/create a simple MQ using a Redis list, queues are named like so "queue:{queueName}"
public async Task<bool> PushToQueue<T>(string queueName, List<T> values)

// pop a message from a queue/List, TCachePopMode.Delete pops remove and removes the first item. Get allows it to remain
public async Task<string> PopFromQueue(string queueName, TCachePopMode popMode = TCachePopMode.Delete)

// typed queue pop
public async Task<T> PopFromQueue<T>(string queueName, TCachePopMode popMode = TCachePopMode.Delete)

// does a queue exist
public async Task<bool> QueueExists(string key)

// remove a queue/list from redis
public async Task<bool> RemoveQueue(string key)

// Search for keys that meet a particular pattern and return their key names.
public async Task<List<string>> SearchKeys(string searchPattern)

// Return the keys and values of the searched objects
public async Task<Dictionary<string, string>> SearchKeyValues(string searchPattern)

// Return the keys and typed values of the searched objects
public async Task<Dictionary<string, T>> SearchKeyValues<T>(string searchPattern)

```
