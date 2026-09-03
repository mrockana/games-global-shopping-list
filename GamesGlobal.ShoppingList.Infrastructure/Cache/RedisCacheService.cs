using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GamesGlobal.ShoppingList.Infrastructure.Cache;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            RedisValue value = await _database.StringGetAsync(key);
            if (value.IsNull)
            {
                _logger.LogInformation("Redis cache miss.");
                return default;
            }

            T? result = JsonSerializer.Deserialize<T>(value.ToString());
            _logger.LogInformation("Redis cache hit.");
            return result;
        }
        catch (RedisException exception)
        {
            _logger.LogWarning(exception, "Redis cache read failed.");
            return default;
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Redis cache value could not be deserialized.");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        try
        {
            string serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue, timeToLive);
        }
        catch (RedisException exception)
        {
            _logger.LogWarning(exception, "Redis cache write failed.");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (RedisException exception)
        {
            _logger.LogWarning(exception, "Redis cache invalidation failed.");
        }
    }
}