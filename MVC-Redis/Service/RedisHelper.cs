using MVC_Redis.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MVC_Redis.Service
{
    public static class RedisHelper
    {
        private static ConnectionMultiplexer _redisconnection;
        private static IDatabase _db;
        const string LockKey = "@#$%";

        static RedisHelper()
        {
            _redisconnection = RedisConnectionFactory.GetConnection();
            _db = _redisconnection.GetDatabase();
        }

        public static List<T> GetCacheData<T>(string key, bool refresh = false) where T : class
        {
            // 取得 memorycache 是否已有 key 的 cahce 資料
            List<T> cacheData = _db.KeyExists(key) ? JsonConvert.DeserializeObject<List<T>>(_db.StringGet(key)) : null;

            //是否強制更新 cache
            if (cacheData != null && refresh)
            {
                _db.KeyDelete(key);
                cacheData = null;
            }

            //cache 不存在
            if (cacheData == null)
                cacheData = SetCacheData<T>(key);

            return cacheData;
        }
        /// <summary>
        /// 設定 data cache
        /// </summary>
        /// <typeparam name="T">Model 型別</typeparam>
        /// <param name="key">caceh key</param>
        /// <returns></returns>
        private static List<T> SetCacheData<T>(string key)
        {
            List<T> cacheData = new List<T>();
            string _lockKey = $"{LockKey}{key}";
            if (_db.StringGet(_lockKey).IsNull && !_db.KeyExists(key))
            {
                //標示為寫入中
                _db.StringSet(_lockKey, true.ToString());

                //將傳入的 model 型別至 db 取得資料後轉為 list，這邊先暫時用 GetSampleData 取代一下
                cacheData = GetSampleData().OfType<T>().ToList();

                //設定 cache 過期時間
                TimeSpan cacheItemPolicy = new TimeSpan(1, 0, 0, 0);

                //加入 cache
                _db.StringSet(key, JsonConvert.SerializeObject(cacheData), cacheItemPolicy);

                //移除寫入中標示
                _db.KeyDelete(_lockKey);
            }
            else
            {
                if (!_db.KeyExists(key))
                    cacheData = SetCacheData<T>(key);
                else
                    cacheData = JsonConvert.DeserializeObject<List<T>>(_db.StringGet(key));
            }
            return cacheData;
        }

        /// <summary>
        /// 代替撈資料那段，因為每次撈的資料型別可能都不同，所以使用型別參數，但範例就先寫死固定 List<Example>
        /// </summary>
        /// <returns></returns>
        private static List<Example> GetSampleData()
        {
            return new List<Example>()
            {
               new Example{Id = Guid.NewGuid(),Name = Guid.NewGuid().ToString()}
            };
        }       
    }
}