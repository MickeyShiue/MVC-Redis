using StackExchange.Redis;
using System;

namespace MVC_Redis.Service
{
    public class RedisConnectionFactory
    {
        private static readonly Lazy<ConnectionMultiplexer> connection;
        public static ConnectionMultiplexer GetConnection() => connection.Value;

        static RedisConnectionFactory()
        {
            var connectionString = "localhost:6379"; //看當初裝 Redis 時用多少Port，預設是 6379
            var options = ConfigurationOptions.Parse(connectionString);

            connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        }
    }
}