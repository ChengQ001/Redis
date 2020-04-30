using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRedisTest
{
    /// <summary>
    /// 需要在AppSettings裡面配置redis連接字符串節點，名稱固定為“redisHost”
    /// 例如，格式參考 value="server=192.168.31.42;port=6380;password=pisenmaster;db=9" 
    /// </summary>
    public class RedisTool : IDisposable
    {

        /// <summary>
        /// redis主機ip
        /// </summary>
        private readonly static string RedisServerIP = "118.24.105.20";// "192.168.31.42";
        /// <summary>
        /// 連接端口
        /// </summary>
        private readonly static int RedisPort = 6379;// 6380;
        /// <summary>
        /// 連接密碼
        /// </summary>
        private readonly static string RedisConnectPassword = "cheng1993";// "pisenmaster";
        /// <summary>
        /// 缓冲池 實例db
        /// </summary>
        private readonly static string PooledRedisDB1 = "cheng1993@118.24.105.20:6379";// string.Empty;// "pisenmaster@192.168.31.42:6380";

        //默认缓存过期时间单位秒
        public const int secondsTimeOut = 60 * 60;

        /// <summary>
        ///  //加載配置文件
        /// </summary>
        static RedisTool()
        {
            string conStr = System.Configuration.ConfigurationManager.AppSettings["redisHost"];
            try
            {
                //if (string.IsNullOrWhiteSpace(conStr))
                //{
                //    throw new Exception("讀取配置文件出錯，AppSettings節沒有配置名為redisHost的redis連接字符串");
                //}
                //string[] arr = conStr.Split(';');
                //RedisServerIP = arr.First(w => w.ToLower().Contains("server="))?.Split('=')[1];
                //RedisPort = Convert.ToInt32(arr.First(w => w.ToLower().Contains("port="))?.Split('=')[1]);
                //RedisConnectPassword = arr.First(w => w.ToLower().Contains("password="))?.Split('=')[1];
                PooledRedisDB1 = conStr;// $"{RedisConnectPassword}@{RedisServerIP}:{RedisPort}";
                Console.WriteLine(PooledRedisDB1);
            }
            catch (Exception ex)
            {
                throw new Exception("讀取配置文件出錯，AppSettings節裡面沒有配置redis連接字符串名為redisHost的節");
            }
        }

        /// <summary>
        /// redis客戶端
        /// </summary>
        public RedisClient Redis = new RedisClient(RedisServerIP, RedisPort, RedisConnectPassword, 0);
        //public RedisClient Redis = new RedisClient("192.168.31.42", 6380, "pisenmaster", 9);

        //缓存池
        private PooledRedisClientManager prcm = new PooledRedisClientManager();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="OpenPooledRedis">是否开启缓冲池</param>
        public RedisTool(bool OpenPooledRedis = false)
        {
            if (OpenPooledRedis)
            {
                //prcm = CreateManager(new[] { "pisenmaster@192.168.31.42:6380" }, new[] { "pisenmaster@192.168.31.42:6380" });
                prcm = CreateManager(new[] { PooledRedisDB1 }, new[] { PooledRedisDB1 });
                Redis = prcm.GetClient() as RedisClient;
            }
        }

        #region Key/Value存储

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">缓存建</param>
        /// <param name="t">缓存值</param>
        /// <param name="timeout">过期时间，单位秒,-1：不过期，0：默认过期时间</param>
        /// <returns></returns>
        public bool Set<T>(string key, T t, int timeout = 0)
        {
            if (timeout < 0)
            {
                //永不過期
                return Redis.Set(key, t);
            }
            if (timeout == 0)
            {
                //默認時長
                timeout = secondsTimeOut;
            }
            return Redis.Set(key, t, TimeSpan.FromSeconds(timeout));
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return Redis.Get<T>(key);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return Redis.Remove(key);
        }

        public bool Add<T>(string key, T t, int timeout)
        {
            if (timeout < 0)
            {
                //永不過期
                return Redis.Set(key, t);
            }
            if (timeout == 0)
            {
                //默認時長
                timeout = secondsTimeOut;
            }
            return Redis.Add(key, t, TimeSpan.FromSeconds(timeout));
        }

        #endregion

        #region 链表操作

        /// <summary>
        /// 根据IEnumerable数据添加链表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="values"></param>
        /// <param name="timeout"></param>
        public void AddList<T>(string listId, IEnumerable<T> values, int timeout = 0)
        {
            IRedisTypedClient<T> iredisClient = Redis.As<T>();
            IRedisList<T> redisList = iredisClient.Lists[listId];
            redisList.AddRange(values);
            if (timeout >= 0)
            {
                if (timeout == 0)
                {
                    timeout = secondsTimeOut;
                }
                Redis.ExpireEntryIn(listId, TimeSpan.FromSeconds(timeout));
            }
            iredisClient.Save();
        }

        /// <summary>
        /// 添加单个实体到链表中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="Item"></param>
        /// <param name="timeout">過期時間會覆蓋列表之前的過期時間,為-1時保持先前的過期設置</param>
        public void AddEntityToList<T>(string listId, T Item, int timeout = 0)
        {
            IRedisTypedClient<T> iredisClient = Redis.As<T>();
            IRedisList<T> redisList = iredisClient.Lists[listId];
            redisList.Add(Item);
            if (timeout >= 0)
            {
                if (timeout == 0)
                {
                    timeout = secondsTimeOut;
                }
                Redis.ExpireEntryIn(listId, TimeSpan.FromSeconds(timeout));
            }
            iredisClient.Save();
        }

        /// <summary>
        /// 获取链表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <returns></returns>
        public IEnumerable<T> GetList<T>(string listId)
        {
            IRedisTypedClient<T> iredisClient = Redis.As<T>();
            return iredisClient.Lists[listId];
        }

        /// <summary>
        /// 在链表中删除单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="t"></param>
        public void RemoveEntityFromList<T>(string listId, T t)
        {
            IRedisTypedClient<T> iredisClient = Redis.As<T>();
            IRedisList<T> redisList = iredisClient.Lists[listId];
            redisList.RemoveValue(t);
            iredisClient.Save();
        }

        /// <summary>
        /// 根据lambada表达式删除符合条件的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="func"></param>
        public void RemoveEntityFromList<T>(string listId, Func<T, bool> func)
        {
            IRedisTypedClient<T> iredisClient = Redis.As<T>();
            IRedisList<T> redisList = iredisClient.Lists[listId];
            T value = redisList.Where(func).FirstOrDefault();
            redisList.RemoveValue(value);
            iredisClient.Save();
        }

        #endregion

        #region 清空Redis所有数据库中的所有key
        public void Flushall()
        {
            Redis.FlushAll();
        }
        #endregion

        //释放资源
        public void Dispose()
        {
            if (Redis != null)
            {
                Redis.Dispose();
                Redis = null;
            }
            GC.Collect();
        }

        /// <summary>
        /// 缓冲池
        /// </summary>
        /// <param name="readWriteHosts"></param>
        /// <param name="readOnlyHosts"></param>
        /// <returns></returns>
        public static PooledRedisClientManager CreateManager(
        string[] readWriteHosts, string[] readOnlyHosts)
        {
            return new PooledRedisClientManager(readWriteHosts, readOnlyHosts,
            new RedisClientManagerConfig
            {
                MaxWritePoolSize = readWriteHosts.Length * 5,
                MaxReadPoolSize = readOnlyHosts.Length * 5,
                AutoStart = true,
            });
            // { RedisClientFactory = (IRedisClientFactory)RedisCacheClientFactory.Instance.CreateRedisClient("127.0.0.1", 6379) }; 
        }
    }

}